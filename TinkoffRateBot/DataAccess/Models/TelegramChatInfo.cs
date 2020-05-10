using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Converters;
using TinkoffRateBot.DataAccess.Interfaces;

namespace TinkoffRateBot.DataAccess.Models
{
    [DynamoDBTable(nameof(TelegramChatInfo))]
    public class TelegramChatInfo : IEntity
    {
        [DynamoDBHashKey(Converter = typeof(DynamoEnumToStringConverter<ChatInfoType>))]
        public ChatInfoType Type { get; set; }
        [DynamoDBRangeKey]
        public long Id { get; set; }
        /// <summary>
        /// Date and time of last update.
        /// </summary>
        [DynamoDBProperty(Converter = typeof(DateTimePropertyConverter))]
        public DateTime Updated { get; set; }
        /// <summary>
        /// Is chat active.
        /// </summary>
        [DynamoDBProperty]
        public bool IsEnabled { get; set; }
        /// <summary>
        /// Threshold of notifications.
        /// </summary>
        [DynamoDBProperty]
        public double DetailedThreshold { get; set; } = 0.5;
        /// <summary>
        /// Minimum of notification's skip range.
        /// </summary>
        [DynamoDBProperty]
        public double MinThresholdRate { get; set; }
        /// <summary>
        /// Maximum of notification's skip range.
        /// </summary>
        [DynamoDBProperty]
        public double MaxThresholdRate { get; set; }

        /// <summary>
        /// Update values of <see cref="MinThresholdRate"/> and <see cref="MaxThresholdRate"/>.
        /// </summary>
        /// <param name="actualRate"></param>
        public void UpdateThresholdRates(double actualRate)
        {
            MinThresholdRate = actualRate - DetailedThreshold;
            MaxThresholdRate = actualRate + DetailedThreshold;
        }
    }
}
