using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TinkoffRateBot.BotCommands
{
    public abstract class TextCommand : IBotCommand
    {
        protected abstract string CommandName { get; }

        public bool CanHandle(Message message) 
            => message?.Type == Telegram.Bot.Types.Enums.MessageType.Text && CanHandle(message.Text);

        public abstract Task HandleAsync(Message message, TelegramBotClient client);

        protected virtual bool CanHandle(string messageText) => messageText.StartsWith($"/{CommandName}");

    }
}
