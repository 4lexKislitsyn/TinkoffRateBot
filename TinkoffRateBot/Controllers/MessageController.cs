using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.Services;

namespace TinkoffRateBot.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> _logger;

        public MessageController(ILogger<MessageController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Post(Update update, [FromServices] TelegramUpdateHandler updateHandler)
        {
            var handled = await updateHandler.Handle(update);
            return handled ? (IActionResult) Ok() : BadRequest("Cannot handle command.");
        }

        [Authorize]
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyChats(string message, [FromServices] TelegramMessageSender botClient, [FromServices] IRepository repository)
        {
            _logger.LogInformation("Start to send accepted message to all active chats.");
            var chats = await repository.GetActiveChatsAsync();
            foreach (var chat in chats)
            {
                await botClient.SendMessageAsync(chat.Id, message);
            }
            return Ok();
        }
    }
}
