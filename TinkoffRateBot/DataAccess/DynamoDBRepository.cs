using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Models;

namespace TinkoffRateBot.DataAccess
{
    public class DynamoDBRepository : Interfaces.IRepository
    {
        private IDynamoDBContext _dynamoDBContext;

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
            var condition = new[] { new ScanCondition(nameof(TelegramChatInfo.IsEnabled), ScanOperator.Equal, true) };
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
    }
}
