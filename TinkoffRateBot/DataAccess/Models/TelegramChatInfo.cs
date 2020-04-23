using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Converters;

namespace TinkoffRateBot.DataAccess.Models
{
    [DynamoDBTable(nameof(TelegramChatInfo))]
    public class TelegramChatInfo
    {
        [DynamoDBHashKey(Converter = typeof(DynamoEnumToStringConverter<ChatInfoType>))]
        public ChatInfoType Type { get; set; }
        [DynamoDBRangeKey]
        public long Id { get; set; }
        [DynamoDBProperty(Converter = typeof(DateTimePropertyConverter))]
        public DateTime Updated { get; set; }
        [DynamoDBProperty]
        public bool IsEnabled { get; set; }
    }
}
