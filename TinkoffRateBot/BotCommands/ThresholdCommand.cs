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
    public class ThresholdCommand : TextCommand
    {
        private readonly IRepository _repository;
        private readonly TelegramMessageSender _messageSender;

        /// <summary>
        /// Create an instance of <see cref="ThresholdCommand"/>.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageSender"></param>
        public ThresholdCommand(IRepository repository, TelegramMessageSender messageSender)
        {
            _repository = repository;
            _messageSender = messageSender;
        }

        /// <inheritdoc/>
        public override async  Task HandleAsync(Message message)
        {
            if (message.Entities?.Length != 1)
            {
                await _messageSender.SendMessageAsync(message.Chat.Id, "Cannot handle this detailed command. Sorry.");
                return;
            }
            var substring = message.Text.Substring(message.Entities[0].Offset + message.Entities[0].Length)?.Trim();
            var isEmptyThreshold = string.IsNullOrWhiteSpace(substring);
            if (!double.TryParse(substring, out var threshold) && !isEmptyThreshold)
            {
                await _messageSender.SendMessageAsync(message.Chat.Id, "Cannot parse threshold.");
                return;
            }
            await _repository.UpdateChatInfo(message.Chat.Id, threshold);
            await _messageSender.SendMessageAsync(message.Chat.Id, $"Threshold of detailed was updated: {threshold}.");
        }
    }
}
