using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IEnumerable<IBotCommand> commands;
        private int _lastUpdateId = -1;

        public TelegramBotGetUpdatesHostedService(ILogger<TelegramBotGetUpdatesHostedService> logger, 
            IOptions<BotConfiguration> options, 
            IRepository repository,
            IEnumerable<IBotCommand> botCommands) 
            : base(logger, options.Value.UpdatesPeriod, 20)
        {
            _botConfiguration = options.Value;
            _repository = repository;
            commands = botCommands;
        }
        protected override async Task TickAsync()
        {
            if (_lastUpdateId < 0)
            {
                var lastUpdate = await _repository.GetLastUpdate();
                _lastUpdateId = (int)lastUpdate.Id;
            }

            var client = new Telegram.Bot.TelegramBotClient(_botConfiguration.Token);
            var updates = await client.GetUpdatesAsync(offset: _lastUpdateId);
            if (updates == null)
            {
                _logger.LogWarning("Updates is null");
                return;
            }

            _lastUpdateId = updates.LastOrDefault()?.Id ?? _lastUpdateId;
            foreach (var update in updates)
            {
                try
                {
                    await commands.FirstOrDefault(x => x.CanHandle(update.Message))?.HandleAsync(update.Message, client);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error was occurred while handling update {Id}", update.Id);
                }
            }
        }
    }
}
