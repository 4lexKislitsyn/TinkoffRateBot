using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Converters;
using TinkoffRateBot.DataAccess.Models;

namespace TinkoffRateBot.DataAccess
{
    public class DynamoDBRepository : Interfaces.IRepository
    {
        private readonly IDynamoDBContext _dynamoDBContext;

        public DynamoDBRepository(IDynamoDBContext dynamoDBContext)
        {
            _dynamoDBContext = dynamoDBContext;
        }

        public async Task<bool> AddAsync(TelegramChatInfo telegramChat)
        {
            await _dynamoDBContext.SaveAsync(telegramChat);
            return true;
        }

        public async Task<bool> AddAsync(TinkoffExchangeRate exchangeRate)
        {
            await _dynamoDBContext.SaveAsync(exchangeRate);
            return true;
        }

        public void Dispose()
        {
            _dynamoDBContext?.Dispose();
        }

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

        public async Task<TelegramChatInfo> GetLastUpdate()
        {
            var asyncQuery = _dynamoDBContext.FromQueryAsync<TelegramChatInfo>(new QueryOperationConfig
            {
                Limit = 1,
                Filter = new QueryFilter(nameof(TelegramChatInfo.Type), QueryOperator.Equal, ChatInfoType.Update.ToString()),
                BackwardSearch = true,
            });
            var items = await asyncQuery.GetNextSetAsync();
            return items.FirstOrDefault();
        }

        public Task<TelegramChatInfo> UpdateChatInfo(long id)
        {
            return _dynamoDBContext.LoadAsync<TelegramChatInfo>(ChatInfoType.Chat.ToString(), id);
        }

        public Task UpdateChatInfo(long id, bool isEnabled)
        {
            return _dynamoDBContext.SaveAsync(new TelegramChatInfo
            {
                Type = ChatInfoType.Chat,
                Updated = DateTime.UtcNow,
                Id = id,
                IsEnabled = isEnabled
            });
        }

        public async Task InitializeDBAsync(IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
            var tables = await client.ListTablesAsync();

            // TODO: use prefix in repository
            var postfix = serviceProvider.GetService<IHostEnvironment>().IsDevelopment() 
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
