using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.DataAccess.Models
{
    public class TelegramChatInfo
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime Added { get; set; }
    }
}
