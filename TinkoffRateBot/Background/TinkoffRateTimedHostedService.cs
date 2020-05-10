using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.DataAccess.Models;
using TinkoffRateBot.Services;

namespace TinkoffRateBot.Background
{
    /// <summary>
    /// Timed service that watching on Tinkoff Bank USD/RUB currency rate.
    /// </summary>
    public class TinkoffRateTimedHostedService : TimedHostedService
    {
        public static TinkoffExchangeRate LastRate { get; private set; }

        private readonly TinkoffRateTimedConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public TinkoffRateTimedHostedService(ILogger<TinkoffRateTimedHostedService> logger, IOptions<TinkoffRateTimedConfiguration> options, IServiceProvider serviceProvider) 
            : base(logger, options.Value.Period, 20)
        {
            _configuration = options.Value;
            _serviceProvider = serviceProvider;
        }
        /// <inheritdoc/>
        protected override async Task TickAsync()
        {
            if (LastRate == null)
            {
                var repository = _serviceProvider.GetRequiredService<IRepository>();
                LastRate = await repository.GetLastRateAsync();
            }

            var client = new HttpClient
            {
                BaseAddress = _configuration.BaseUrl
            };

            var response = await client.GetAsync("v1/currency_rates?from=USD&to=RUB");

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogCritical($"Can't get rate from API. Status: {response.StatusCode}; Content: {content}");
                return;
            }

            var obj = JObject.Parse(content);
            var rate = obj.SelectToken($"$.payload.rates[?(@.category == '{_configuration.CategoryName}')]");
            if (rate == null)
            {
                _logger.LogCritical($"Can't get rate width category SavingAccountTransfers. Content: {content}.");
                return;
            }
            var parsedRate = rate.ToObject<TinkoffExchangeRate>();
            parsedRate.From = rate.SelectToken("fromCurrency.name").Value<string>();
            parsedRate.Updated = DateTime.UnixEpoch.AddMilliseconds(obj.SelectToken("$.payload.lastUpdate.milliseconds").Value<double>());

            var diff = Math.Abs(parsedRate.Sell - (LastRate?.Sell ?? parsedRate.Sell));
            if (diff == 0)
            {
                if (LastRate == null)
                {
                    await _serviceProvider.GetRequiredService<IRepository>().SaveEntityAsync(parsedRate);
                    LastRate = parsedRate;
                }
                else
                {
                    _logger.LogInformation($"Difference between parsed and last rate is 0.");
                }
                return;
            }

            await _serviceProvider.GetRequiredService<IRepository>().SaveEntityAsync(parsedRate);
            _logger.LogInformation($"Rate added to DB: {parsedRate.GetDiffMessage(LastRate)}");
            var sender = _serviceProvider.GetRequiredService<TelegramMessageSender>();
            await sender.SendDetailedRate(parsedRate, LastRate);
            LastRate = parsedRate;
        }
    }
}
