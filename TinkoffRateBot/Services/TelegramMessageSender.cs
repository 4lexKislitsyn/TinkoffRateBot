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
        private readonly ITelegramBotClient _client;
        private readonly IRepository _repository;
        private readonly ILogger<TelegramMessageSender> _logger;

        public TelegramMessageSender(ILogger<TelegramMessageSender> logger, ITelegramBotClient client, IRepository repository)
        {
            _client = client;
            _repository = repository;
            _logger = logger;
        }

        public async Task SendRateAsync(TinkoffExchangeRate currentRate, TinkoffExchangeRate previousRate = null, ITelegramBotClient client = null)
        {
            var chats = await _repository.GetActiveChatsAsync();
            foreach (var chat in chats)
            {
                await SendRateAsync(chat.Id, currentRate, previousRate, client);
            }
        }

        public async Task SendRateAsync(long chatId, TinkoffExchangeRate currentRate, TinkoffExchangeRate prevRate = null, ITelegramBotClient client = null)
        {
            var message = prevRate != null ? $"**UPDATE**\n{currentRate.From}:  {currentRate.GetDiffMessage(prevRate)}"
                : $"{currentRate.From}: {currentRate.Sell}";
            try
            {
                await SendMessageAsync(chatId, message, client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while sending message to chat {chatId}");
            }
        }

        public async Task SendMessageAsync(long chatId, string markdownMessage, ITelegramBotClient client = null)
        {
            try
            {
                await (client ?? _client).SendTextMessageAsync(new ChatId(chatId), markdownMessage, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                _logger.LogInformation($"Message was sent in chat {chatId} ({markdownMessage})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while sending message to chat {chatId}");
            }
        }

        public async Task SendDetailedRate(TinkoffExchangeRate parsedRate, TinkoffExchangeRate lastRate)
        {
            var chats = await _repository.GetDetailedChatsAsync(parsedRate, lastRate);
            foreach (var chat in chats)
            {
                await _repository.UpdateChatInfo(chat.Id, chat.DetailedThreshold, parsedRate.Sell);
                await SendMessageAsync(chat.Id, $"Difference from active rate: {parsedRate.GetDiffMessage(lastRate)}");
            }
        }
    }
}
