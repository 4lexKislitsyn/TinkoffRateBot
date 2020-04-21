using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.DataAccess.Models
{
    public class TinkoffExchangeRate
    {
        public string From { get; set; }
        public string Category { get; set; }
        public double Sell { get; set; }
        public double Buy { get; set; }
    }
}
