using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TinkoffRateBot.BotCommands
{
    public interface IBotCommand
    {
        bool CanHandle(Message message);

        Task HandleAsync(Message message, TelegramBotClient client);
    }
}
