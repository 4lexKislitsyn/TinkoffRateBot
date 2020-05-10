using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TinkoffRateBot.BotCommands
{
    /// <summary>
    /// Interface for Telegram Bot commands.
    /// </summary>
    public interface IBotCommand
    {
        /// <summary>
        /// Check if command can handle message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool CanHandle(Message message);
        /// <summary>
        /// Handle of message command if <see cref="CanHandle(Message)"/> return <see langword="true"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        Task HandleAsync(Message message);
    }
}
