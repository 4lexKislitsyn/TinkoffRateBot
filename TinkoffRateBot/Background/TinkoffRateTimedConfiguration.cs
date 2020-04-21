using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.Background
{
    public class TinkoffRateTimedConfiguration
    {
        public Uri BaseUrl { get; set; }
        public int Period { get; set; }
        public double Threshold { get; set; }
        public string CategoryName { get; set; }
    }
}
