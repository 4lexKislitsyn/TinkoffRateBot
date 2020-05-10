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
using TinkoffRateBot.Models;
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
            return Ok(handled);
        }

        [Authorize]
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyChats(Notify notifyContent, [FromServices] TelegramMessageSender botClient, [FromServices] IRepository repository)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Start to send accepted message to all active chats.");
            if (notifyContent.ChatId.HasValue)
            {
                await botClient.SendMessageAsync(notifyContent.ChatId.Value, notifyContent.Message);
            }
            else
            {

                var chats = await repository.GetActiveChatsAsync(); 
                foreach (var chat in chats)
                {
                    await botClient.SendMessageAsync(chat.Id, notifyContent.Message);
                }
            }
            
            return Ok();
        }
    }
}
