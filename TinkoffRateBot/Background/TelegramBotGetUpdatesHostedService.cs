using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TinkoffRateBot.BotCommands;
using TinkoffRateBot.DataAccess.Interfaces;

namespace TinkoffRateBot.Background
{
    public class TelegramBotGetUpdatesHostedService : TimedHostedService
    {
        private readonly BotConfiguration _botConfiguration;
        private readonly IRepository _repository;
        private readonly IEnumerable<IBotCommand> _commands;
        private int _lastUpdateId = -1;

        public TelegramBotGetUpdatesHostedService(ILogger<TelegramBotGetUpdatesHostedService> logger, 
            IOptions<BotConfiguration> options, 
            IRepository repository,
            IEnumerable<IBotCommand> botCommands) 
            : base(logger, options.Value.UpdatesPeriod, 20)
        {
            _botConfiguration = options.Value;
            _repository = repository;
            _commands = botCommands;
        }
        protected override async Task TickAsync()
        {
            if (_lastUpdateId < 0)
            {
                var lastUpdate = await _repository.GetLastUpdate();
                _lastUpdateId = (int)lastUpdate.Id;
            }
            _logger.LogInformation($"LastUpdateId = {_lastUpdateId}");
            var client = new Telegram.Bot.TelegramBotClient(_botConfiguration.Token);
            _logger.LogInformation("Trying to get updates");
            var updates = await client.GetUpdatesAsync(offset: _lastUpdateId + 1);
            if (updates == null)
            {
                _logger.LogWarning("Updates is null");
                return;
            }
            _logger.LogInformation($"Got updates = {updates.Count()}");
            _lastUpdateId = updates.LastOrDefault()?.Id ?? _lastUpdateId;
            foreach (var update in updates)
            {
                try
                {
                    _logger.LogInformation($"Try to handle message {update?.Message?.Text}");
                    var command = _commands.FirstOrDefault(x => x.CanHandle(update.Message));
                    if (command == null)
                    {
                        _logger.LogWarning($"Command wasn't found. Type = {update.Message.Type}.");
                        continue;
                    }
                    await command.HandleAsync(update.Message, client);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error was occurred while handling update {update.Id} ({update.Message}); commands count = {_commands?.Count()}");
                }
            }
            await _repository.AddAsync(new DataAccess.Models.TelegramChatInfo { Id = _lastUpdateId, Type = DataAccess.Models.ChatInfoType.Update });
        }
    }
}
