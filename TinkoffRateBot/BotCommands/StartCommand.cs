﻿using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TinkoffRateBot.Background;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.Services;

namespace TinkoffRateBot.BotCommands
{
    public class StartCommand : TextCommand
    {
        private readonly ILogger<StartCommand> _logger;
        private readonly IRepository _repository;
        private readonly TelegramMessageSender _messageSender;

        public StartCommand(ILogger<StartCommand> logger, IRepository repository, TelegramMessageSender messageSender)
        {
            _logger = logger;
            _repository = repository;
            _messageSender = messageSender;
        }
        public override async Task HandleAsync(Message message, TelegramBotClient client)
        {
            await _repository.UpdateChatInfo(message.Chat.Id, true);
            await _messageSender.SendRateAsync(message.Chat.Id, TinkoffRateTimedHostedService.LastRate);
        }

        protected override bool CanHandle(string messageText)
        {
            return messageText.StartsWith("./start");
        }
    }
}
