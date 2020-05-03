using System.Collections.Generic;
using Telegram.Bot.Types;

namespace TinkoffRateBot
{
    public partial class BasicAuthHandlerConfiguration
    {
        public IEnumerable<BasicAuthUser> Users { get; set; }
    }
}