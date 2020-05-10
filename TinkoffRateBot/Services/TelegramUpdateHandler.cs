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
    /// <summary>
    /// Handle telegram request with <see cref="Update"/> content.
    /// </summary>
    public class TelegramUpdateHandler
    {
        private readonly ILogger<TelegramUpdateHandler> _logger;
        private readonly IEnumerable<IBotCommand> _botCommands;
        private readonly TelegramMessageSender _telegramMessageSender;

        /// <summary>
        /// Create an instance of <see cref="TelegramUpdateHandler"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="botCommands"></param>
        public TelegramUpdateHandler(ILogger<TelegramUpdateHandler> logger, IEnumerable<IBotCommand> botCommands, TelegramMessageSender telegramMessageSender)
        {
            _logger = logger;
            _botCommands = botCommands;
            _telegramMessageSender = telegramMessageSender;
        }
        /// <summary>
        /// Handle Telegram update message.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public async ValueTask<bool> Handle(Update update)
        {
            try
            {
                _logger.LogInformation($"Try to handle message {update?.Message?.Text} ({update.Message.Chat?.Id})");
                var command = _botCommands.FirstOrDefault(x => x.CanHandle(update.Message));
                if (command == null)
                {
                    _logger.LogWarning($"Command wasn't found. Type = {update.Message.Type}.");
                    await _telegramMessageSender.SendMessageAsync(update.Message.Chat.Id, "Unknown command. Sorry. Send /start to enable notifications.");
                    return false;
                }
                await command.HandleAsync(update.Message);
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
