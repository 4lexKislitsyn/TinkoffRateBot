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

        public async Task SendRateAsync(long chatId, TinkoffExchangeRate currentRate, ITelegramBotClient client = null)
        {
            var message = $"{currentRate.From}: {currentRate.Sell}";
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

        public async Task SendRateUpdate(TinkoffExchangeRate parsedRate, TinkoffExchangeRate lastRate)
        {
            var chats = await _repository.GetDetailedChatsAsync(parsedRate, lastRate);
            foreach (var chat in chats)
            {
                await SendMessageAsync(chat.Id, $"**UPDATE**\n{parsedRate.From}:  {parsedRate.GetDiffMessage(chat.MinThresholdRate + chat.DetailedThreshold)}");
                await _repository.UpdateChatInfo(chat.Id, chat.DetailedThreshold, parsedRate.Sell);
            }
        }
    }
}
