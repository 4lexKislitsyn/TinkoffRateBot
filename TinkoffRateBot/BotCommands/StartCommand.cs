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

        protected override string CommandName => "start";

        public override async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            _logger.LogInformation($"Handle START command for chat {message.Chat.Id}.");
            await _repository.UpdateChatInfo(message.Chat.Id, true);
            _logger.LogInformation("Start sending START message.");
            await _messageSender.SendRateAsync(message.Chat.Id, TinkoffRateTimedHostedService.LastRate);
        }
    }
}
