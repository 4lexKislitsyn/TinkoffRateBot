using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.Services;

namespace TinkoffRateBot.BotCommands
{
    /// <summary>
    /// Command to pause notifications.
    /// </summary>
    public class PauseCommand : TextCommand
    {
        private readonly ILogger<StartCommand> _logger;
        private readonly IRepository _repository;
        private readonly TelegramMessageSender _messageSender;

        /// <summary>
        /// Create an instance of <see cref="PauseCommand"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="repository"></param>
        /// <param name="messageSender"></param>
        public PauseCommand(ILogger<StartCommand> logger, IRepository repository, TelegramMessageSender messageSender)
        {
            _logger = logger;
            _repository = repository;
            _messageSender = messageSender;
        }

        /// <inheritdoc/>
        public async override Task HandleAsync(Message message)
        {
            _logger.LogTrace($"Start handle PAUSE command for chat {message.Chat.Id}");
            await _repository.UpdateChatInfo(message.Chat.Id, false);
            _logger.LogTrace("Sending message from PAUSE command");
            await _messageSender.SendMessageAsync(message.Chat.Id, "Notifications was paused. Send /start to resume notifications.");
        }
    }
}
