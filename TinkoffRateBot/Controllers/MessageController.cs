using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.Services;

namespace TinkoffRateBot.Controllers
{
    [Route("api/message")]
    public class MessageController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post(Update update, [FromServices] TelegramUpdateHandler updateHandler)
        {
            var handled = await updateHandler.Handle(update);
            return handled ? (IActionResult) Ok() : BadRequest("Cannot handle command.");
        }

        [Authorize]
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyChats(string message, [FromServices] ITelegramBotClient botClient, [FromServices] IRepository repository)
        {
            var chats = await repository.GetActiveChatsAsync();
            foreach (var chat in chats)
            {
                await botClient.SendTextMessageAsync(chat.Id, message, Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            return Ok();
        }
    }
}
