using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TinkoffRateBot.BotCommands;

namespace TinkoffRateBot.Services
{
    public class TelegramUpdateHandler
    {
        private readonly ILogger<TelegramUpdateHandler> _logger;
        private readonly IEnumerable<IBotCommand> _botCommands;
        private readonly ITelegramBotClient _botClient;

        public TelegramUpdateHandler(ILogger<TelegramUpdateHandler> logger, IEnumerable<IBotCommand> botCommands, ITelegramBotClient botClient)
        {
            _logger = logger;
            _botCommands = botCommands;
            _botClient = botClient;
        }

        public async ValueTask<bool> Handle(Update update)
        {
            try
            {
                _logger.LogInformation($"Try to handle message {update?.Message?.Text}");
                var command = _botCommands.FirstOrDefault(x => x.CanHandle(update.Message));
                if (command == null)
                {
                    _logger.LogWarning($"Command wasn't found. Type = {update.Message.Type}.");
                    return false;
                }
                await command.HandleAsync(update.Message, _botClient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error was occurred while handling update {update.Id} ({update.Message}); commands count = {_botCommands?.Count()}");
                return false;
            }
        } 
    }
}
