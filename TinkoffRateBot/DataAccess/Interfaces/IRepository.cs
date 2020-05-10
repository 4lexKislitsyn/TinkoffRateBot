using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Models;

namespace TinkoffRateBot.DataAccess.Interfaces
{
    /// <summary>
    /// Database actions provider.
    /// </summary>
    public interface IRepository : IDisposable
    {
        /// <summary>
        /// Save entity to database. Automatically changes <see cref="IEntity.Updated"/> property.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task SaveEntityAsync(IEntity entity);
        /// <summary>
        /// Get last currency rate.
        /// </summary>
        /// <returns></returns>
        Task<TinkoffExchangeRate> GetLastRateAsync();
        /// <summary>
        /// Get chats with <see cref="TelegramChatInfo.IsEnabled"/> set to <see langword="true"/>.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TelegramChatInfo>> GetActiveChatsAsync();
        /// <summary>
        /// Initialize DB.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        Task InitializeDBAsync(IServiceProvider serviceProvider);
        /// <summary>
        /// Update <see cref="TelegramChatInfo.IsEnabled"/> property value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        Task UpdateChatInfo(long id, bool isEnabled);
        /// <summary>
        /// Update chat properties: <see cref="TelegramChatInfo.DetailedThreshold"/>, <see cref="TelegramChatInfo.MinThresholdRate"/> and <see cref="TelegramChatInfo.MaxThresholdRate"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="threshold"></param>
        /// <param name="actualRate"></param>
        /// <returns></returns>
        Task UpdateChatInfo(long id, double threshold, double? actualRate = null);
        /// <summary>
        /// Get collection of chats that should be notified about rate changing.
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        Task<IEnumerable<TelegramChatInfo>> GetDetailedChatsAsync(TinkoffExchangeRate actual, TinkoffExchangeRate previous);
    }
}
