using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TinkoffRateBot.DataAccess.Interfaces;

namespace TinkoffRateBot.BotCommands
{
    public class DetailedCommand : TextCommand
    {
        private readonly IRepository _repository;

        public DetailedCommand(IRepository repository)
        {
            _repository = repository;
        }
        protected override string CommandName => "detailed";

        public override async  Task HandleAsync(Message message, ITelegramBotClient client)
        {
            if (message.Entities?.Length != 1)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Cannot handle this detailed command. Sorry.");
                return;
            }
            var substring = message.Text.Substring(message.Entities[0].Offset + message.Entities[0].Length)?.Trim();
            var isEmptyThreshold = string.IsNullOrWhiteSpace(substring);
            if (!double.TryParse(substring, out var threshold) && !isEmptyThreshold)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Cannot parse threshold.");
                return;
            }
            await _repository.UpdateChatInfo(message.Chat.Id, threshold);
            await client.SendTextMessageAsync(message.Chat.Id, $"Threshold of detailed was updated: {threshold}.");
        }
    }
}
