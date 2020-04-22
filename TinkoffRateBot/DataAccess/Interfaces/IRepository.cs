using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Models;

namespace TinkoffRateBot.DataAccess.Interfaces
{
    public interface IRepository : IDisposable
    {
        Task<bool> AddAsync(TelegramChatInfo telegramChat);
        Task<bool> AddAsync(TinkoffExchangeRate exchangeRate);
        Task<TinkoffExchangeRate> GetLastRateAsync();
        Task<IEnumerable<TelegramChatInfo>> GetActiveChatsAsync();
        Task InitializeDBAsync(IServiceProvider serviceProvider);
    }
}
