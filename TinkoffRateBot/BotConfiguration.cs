using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot
{
    public class BotConfiguration
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public Uri WebHookUrl { get; set; }
    }
}
