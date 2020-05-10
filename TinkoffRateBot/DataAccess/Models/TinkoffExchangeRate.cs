using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Converters;
using TinkoffRateBot.DataAccess.Interfaces;

namespace TinkoffRateBot.DataAccess.Models
{
    [DynamoDBTable(nameof(TinkoffExchangeRate))]
    public class TinkoffExchangeRate : IEntity
    {
        [DynamoDBHashKey]
        public string From { get; set; }
        public string Category { get; set; }
        [DynamoDBProperty]
        public double Sell { get; set; }
        [DynamoDBProperty]
        public double Buy { get; set; }
        [DynamoDBRangeKey(Converter = typeof(DateTimePropertyConverter))]
        public DateTime Updated { get; set; }

        /// <summary>
        /// Get formated message with info about current <see cref="Sell"/> value and difference between <paramref name="compareRate"/>.
        /// </summary>
        /// <param name="compareRate"></param>
        /// <returns></returns>
        public string GetDiffMessage(TinkoffExchangeRate compareRate) => GetDiffMessage(compareRate?.Sell ?? 0);
        /// <summary>
        /// Get formated message with info about current <see cref="Sell"/> value and difference between <paramref name="compareRateSellValue"/>.
        /// </summary>
        /// <param name="compareRateSellValue"></param>
        /// <returns></returns>
        public string GetDiffMessage(double compareRateSellValue) => $"{Sell} ({Sell - compareRateSellValue:+0.##;-0.##;0})";
    }
}
