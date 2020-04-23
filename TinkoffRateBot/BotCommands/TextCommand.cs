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
        public bool CanHandle(Message message) 
            => message?.Type == Telegram.Bot.Types.Enums.MessageType.Text && CanHandle(message.Text);

        public abstract Task HandleAsync(Message message, TelegramBotClient client);

        protected abstract bool CanHandle(string messageText);
    }
}
