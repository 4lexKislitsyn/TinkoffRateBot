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
            var random = new Random((int)DateTime.Now.Ticks);
            var rate = new TinkoffExchangeRate
            {
                From = "USD",
                Buy = random.NextDouble(),
                Sell = random.NextDouble(),
                Updated = DateTime.Now,
                Category = "SavingAccountTransfers"
            };
            await repository.AddAsync(rate);
            await repository.AddAsync(new TelegramChatInfo
            {
                Id = DateTime.Now.Second,
                Added = DateTime.UtcNow,
                IsEnabled = true,
            });
            return Ok(await repository.GetLastRateAsync());
        }
    }
}
