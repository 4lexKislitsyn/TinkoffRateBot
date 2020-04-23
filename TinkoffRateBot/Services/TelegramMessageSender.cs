using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.DataAccess.Models;

namespace TinkoffRateBot.Services
{
    public class TelegramMessageSender
    {
        private readonly BotConfiguration _configuration;
        private readonly IRepository _repository;
        private readonly ILogger<TelegramMessageSender> _logger;

        public TelegramMessageSender(ILogger<TelegramMessageSender> logger, IOptions<BotConfiguration> options, IRepository repository)
        {
            _configuration = options.Value;
            _repository = repository;
            _logger = logger;
        }

        public async Task SendRateAsync(TinkoffExchangeRate currentRate, TinkoffExchangeRate previousRate = null)
        {
            var chats = await _repository.GetActiveChatsAsync();
            foreach (var chat in chats)
            {
                await SendRateAsync(chat.Id, currentRate, previousRate);
            }
        }

        public async Task SendRateAsync(long chatId, TinkoffExchangeRate currentRate, TinkoffExchangeRate prevRate = null)
        {
            var message = prevRate != null ? $"**UPDATE**\n{currentRate.From}:  {currentRate.Sell} ({currentRate.Sell - prevRate.Sell:+0.##;-0.##;0}"
                : $"{currentRate.From}: {currentRate.Sell}";
            try
            {
                await SendMessageAsync(chatId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while sending message to chat {chatId}");
            }
        }


        public async Task SendMessageAsync(long chatId, string markdownMessage)
        {
            try
            {
                await new TelegramBotClient(_configuration.Token).SendTextMessageAsync(new ChatId(chatId), markdownMessage, Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while sending message to chat {chatId}");
            }
        }
    }
}
