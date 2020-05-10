using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Converters;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.DataAccess.Models;

namespace TinkoffRateBot.DataAccess
{
    /// <summary>
    /// DynamoDB database actions provider.
    /// </summary>
    public class DynamoDBRepository : IRepository
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly ILogger<DynamoDBRepository> _logger;
        /// <summary>
        /// Create an instance of <see cref="DynamoDBRepository"/>.
        /// </summary>
        /// <param name="dynamoDBContext"></param>
        /// <param name="logger"></param>
        public DynamoDBRepository(IDynamoDBContext dynamoDBContext, ILogger<DynamoDBRepository> logger)
        {
            _dynamoDBContext = dynamoDBContext;
            _logger = logger;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            _dynamoDBContext?.Dispose();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<TelegramChatInfo>> GetActiveChatsAsync()
        {
            var condition = new[] 
            { 
                new ScanCondition(nameof(TelegramChatInfo.Type), ScanOperator.Equal, ChatInfoType.Chat.ToString()), 
                new ScanCondition(nameof(TelegramChatInfo.IsEnabled), ScanOperator.Equal, true) 
            };
            var scan = _dynamoDBContext.ScanAsync<TelegramChatInfo>(condition);
            var chats = new List<TelegramChatInfo>();
            while (!scan.IsDone)
            {
                chats.AddRange(await scan.GetNextSetAsync());
            }
            
            return chats;
        }
        /// <inheritdoc/>
        public async Task<TinkoffExchangeRate> GetLastRateAsync()
        {
            var asyncQuery = _dynamoDBContext.FromQueryAsync<TinkoffExchangeRate>(new QueryOperationConfig
            {
                Limit = 1,
                Filter = new QueryFilter(nameof(TinkoffExchangeRate.From), QueryOperator.Equal, "USD"),
                BackwardSearch = true,
            });
            var items = await asyncQuery.GetNextSetAsync();
            return items.FirstOrDefault();
        }
        /// <inheritdoc/>
        public async Task UpdateChatInfo(long id, bool isEnabled)
        {
            var chatInfo = await _dynamoDBContext.LoadAsync<TelegramChatInfo>(ChatInfoType.Chat.ToString(), id);

            if (chatInfo == null)
            {
                if (!isEnabled)
                {
                    _logger.LogWarning($"Trying to pause unknown chat ({id})");
                    return;
                }
                chatInfo = new TelegramChatInfo { Id = id };
            }

            chatInfo.IsEnabled = isEnabled;
            await SaveEntityAsync(chatInfo);
        }
        /// <inheritdoc/>
        public async Task UpdateChatInfo(long id, double threshold, double? actualRate = null)
        {
            var chatInfo = await _dynamoDBContext.LoadAsync<TelegramChatInfo>(ChatInfoType.Chat.ToString(), id);

            if (chatInfo == null)
            {
                _logger.LogWarning("Trying to update threshold of unknown chat.");
                return;
            }

            chatInfo.DetailedThreshold = threshold;

            actualRate ??= (await GetLastRateAsync())?.Sell;
            if (actualRate.HasValue)
            {
                chatInfo.UpdateThresholdRates(actualRate.Value);
            }

            await SaveEntityAsync(chatInfo);
        }
        /// <inheritdoc/>
        public async Task InitializeDBAsync(IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
            var tables = await client.ListTablesAsync();

            // TODO: use prefix in repository
            var env = serviceProvider.GetService<IHostEnvironment>();
            var postfix = env.IsDevelopment() 
                ? string.Empty
                : string.Empty;

            async Task CreateIfNotExist<T>()
            {
                var tableName = typeof(T).Name + postfix;
                if (tables.TableNames.Contains(tableName))
                {
                    return;
                }
                var createRequest = CreateTableRequest<T>(postfix);
                var response = await client.CreateTableAsync(createRequest);
                var tableStatus = response.TableDescription.TableStatus;
                while (tableStatus != TableStatus.ACTIVE)
                {
                    await Task.Delay(3000);
                    var descriveResponse = await client.DescribeTableAsync(tableName);
                    tableStatus = descriveResponse.Table.TableStatus;
                }
            }
            await CreateIfNotExist<TelegramChatInfo>();
            await CreateIfNotExist<TinkoffExchangeRate>();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<TelegramChatInfo>> GetDetailedChatsAsync(TinkoffExchangeRate actual, TinkoffExchangeRate previous)
        {
            var condition = actual.Sell - previous.Sell > 0
                ? new ScanCondition(nameof(TelegramChatInfo.MaxThresholdRate), ScanOperator.LessThanOrEqual, actual.Sell)
                : new ScanCondition(nameof(TelegramChatInfo.MinThresholdRate), ScanOperator.GreaterThanOrEqual, actual.Sell);

            var conditions = new[]
            {
                new ScanCondition(nameof(TelegramChatInfo.IsEnabled), ScanOperator.Equal, true),
                new ScanCondition(nameof(TelegramChatInfo.Type), ScanOperator.Equal, ChatInfoType.Chat.ToString()),
                condition
            };
            var scan = _dynamoDBContext.ScanAsync<TelegramChatInfo>(conditions);
            var result = new List<TelegramChatInfo>();
            while (!scan.IsDone)
            {
                result.AddRange(await scan.GetNextSetAsync());
            }
            return result;
        }
        /// <inheritdoc/>
        public async Task SaveEntityAsync(IEntity entity)
        {
            entity.Updated = DateTime.UtcNow;
            switch (entity)
            {
                case TelegramChatInfo telegramChat:
                    if (telegramChat.MinThresholdRate <= 0 || telegramChat.MaxThresholdRate <= 0)
                    {
                        var lastRate = await GetLastRateAsync();
                        if (lastRate != null)
                        {
                            telegramChat.UpdateThresholdRates(lastRate.Sell);
                        }
                    }
                    await _dynamoDBContext.SaveAsync(telegramChat);
                    break;
                case TinkoffExchangeRate rate:
                    await _dynamoDBContext.SaveAsync(rate);
                    break;
            }
        }

        private static IEnumerable<KeySchemaElement> GetKeys<T>()
        {
            var properties = typeof(T).GetProperties();

            var hashKey = properties.FirstOrDefault(x => Attribute.IsDefined(x, typeof(DynamoDBHashKeyAttribute)));

            yield return new KeySchemaElement(hashKey.Name, "HASH");

            foreach (var prop in properties.Where(x => Attribute.IsDefined(x, typeof(DynamoDBRangeKeyAttribute))))
            {
                yield return new KeySchemaElement(prop.Name, "RANGE");
            }
        }

        private static IEnumerable<AttributeDefinition> GetAttributes<T>()
        {
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties.Where(x => Attribute.IsDefined(x, typeof(DynamoDBHashKeyAttribute)) || Attribute.IsDefined(x, typeof(DynamoDBRangeKeyAttribute))))
            {
                var isStringEnum = false;
                if (prop.PropertyType.IsEnum)
                {
                    var attributeType = prop.CustomAttributes.FirstOrDefault(x => x.AttributeType.BaseType == typeof(DynamoDBPropertyAttribute))?.AttributeType;
                    if (attributeType != null)
                    {
                        var attribute = Attribute.GetCustomAttribute(prop, attributeType) as DynamoDBPropertyAttribute;
                        isStringEnum = attribute.Converter?.IsGenericType == true 
                            && attribute.Converter.GetGenericTypeDefinition() == typeof(DynamoEnumToStringConverter<>);
                    }
                }
                string type;
                if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime) || isStringEnum)
                {
                    type = "S";
                }
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(float) || prop.PropertyType == typeof(long))
                {
                    type = "N";
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    type = "N";
                }
                else
                {
                    type = "B";
                }
                yield return new AttributeDefinition(prop.Name, type);
            }
        }

        private static CreateTableRequest CreateTableRequest<T>(string postfix) => new CreateTableRequest(typeof(T).Name + postfix, GetKeys<T>().ToList(), GetAttributes<T>().ToList(), new ProvisionedThroughput
        {
            ReadCapacityUnits = 5,
            WriteCapacityUnits = 5
        });
    }
}
