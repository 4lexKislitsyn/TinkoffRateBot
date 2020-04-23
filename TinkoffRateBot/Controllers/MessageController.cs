using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using TinkoffRateBot.DataAccess.Interfaces;
using TinkoffRateBot.DataAccess.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TinkoffRateBot.Controllers
{
    [Route("api/message")]
    public class MessageController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] IRepository repository)
        {
            await repository.AddAsync(new TelegramChatInfo
            {
                Type = ChatInfoType.Chat,
                Id = 53,
                Updated = DateTime.UtcNow,
                IsEnabled = true,
            });
            return Ok(await repository.GetLastRateAsync());
        }
    }
}
