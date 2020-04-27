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
    public class TinkoffRateTimedHostedService : TimedHostedService
    {
        private readonly TinkoffRateTimedConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public static TinkoffExchangeRate LastRate { get; private set; }

        public TinkoffRateTimedHostedService(ILogger<TinkoffRateTimedHostedService> logger, IOptions<TinkoffRateTimedConfiguration> options, IServiceProvider serviceProvider) 
            : base(logger, options.Value.Period, 20)
        {
            _configuration = options.Value;
            _serviceProvider = serviceProvider; 
        }

        protected override async Task TickAsync()
        {
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
            var parsedRate = new TinkoffExchangeRate()
            {
                Buy = rate.SelectToken("buy").Value<double>(),
                Sell = rate.SelectToken("sell").Value<double>(),
                From = rate.SelectToken("fromCurrency.name").Value<string>(),
                Updated = DateTime.UnixEpoch.AddMilliseconds(obj.SelectToken("$.payload.lastUpdate.milliseconds").Value<double>()),
                Category = rate.SelectToken("category").Value<string>(),
            };

            if (LastRate == null)
            {
                var repository = _serviceProvider.GetRequiredService<IRepository>();
                LastRate = await repository.GetLastRateAsync();
            }
            var diff = Math.Abs(parsedRate.Sell - (LastRate?.Sell ?? 0));
            var sender = _serviceProvider.GetRequiredService<TelegramMessageSender>();
            if (LastRate == null || diff >= _configuration.Threshold)
            {
                await _serviceProvider.GetRequiredService<IRepository>().AddAsync(parsedRate);
                _logger.LogInformation($"Rate added to DB: {parsedRate.GetDiffMessage(LastRate)}");
                await sender.SendRateAsync(parsedRate, LastRate);
                LastRate = parsedRate;
            }
            else
            {
                await sender.SendDetailedRate(parsedRate, LastRate);
                _logger.LogInformation($"Rate is skipped: {parsedRate.GetDiffMessage(LastRate)}");
            }
        }
    }
}
