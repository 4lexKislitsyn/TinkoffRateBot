using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TinkoffRateBot.BotCommands
{
    /// <summary>
    /// Command based on text message.
    /// </summary>
    public abstract class TextCommand : IBotCommand
    {
        /// <summary>
        /// The name of the command with which message should start.
        /// </summary>
        protected virtual string CommandName => GetType().Name.Replace("Command", string.Empty).ToLower();

        /// <inheritdoc/>
        public bool CanHandle(Message message) 
            => message?.Type == Telegram.Bot.Types.Enums.MessageType.Text && CanHandle(message.Text);

        /// <inheritdoc/>
        public abstract Task HandleAsync(Message message);

        /// <summary>
        /// Check if command can handle text message.
        /// </summary>
        /// <param name="messageText"></param>
        /// <returns></returns>
        protected virtual bool CanHandle(string messageText) => messageText.StartsWith($"/{CommandName}");

    }
}
