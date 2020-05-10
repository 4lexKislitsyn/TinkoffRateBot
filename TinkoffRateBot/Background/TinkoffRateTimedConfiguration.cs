using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.Background
{
    public class TinkoffRateTimedConfiguration
    {
        /// <summary>
        /// Address of API.
        /// </summary>
        public Uri BaseUrl { get; set; }
        /// <summary>
        /// Currency scan period.
        /// </summary>
        public int Period { get; set; }
        /// <summary>
        /// Category to watch.
        /// </summary>
        public string CategoryName { get; set; }
    }
}
