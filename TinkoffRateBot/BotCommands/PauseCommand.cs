﻿using Microsoft.Extensions.Logging;
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
    public class PauseCommand : TextCommand
    {
        private readonly ILogger<StartCommand> _logger;
        private readonly IRepository _repository;
        private readonly TelegramMessageSender _messageSender;

        public PauseCommand(ILogger<StartCommand> logger, IRepository repository, TelegramMessageSender messageSender)
        {
            _logger = logger;
            _repository = repository;
            _messageSender = messageSender;
        }
        public async override Task HandleAsync(Message message, TelegramBotClient client)
        {
            await _repository.UpdateChatInfo(message.Chat.Id, false);
            await _messageSender.SendMessageAsync(message.Chat.Id, "Send /start to resume notifications.");
        }

        protected override bool CanHandle(string messageText) => messageText.StartsWith("/pause");
    }
}