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
        [DynamoDBHashKey]
        public int Id { get; set; }
        [DynamoDBProperty]
        public bool IsEnabled { get; set; }
        [DynamoDBRangeKey(Converter = typeof(DateTimePropertyConverter))]
        public DateTime Added { get; set; }

        //[DynamoDBIgnore]
        //public DateTime? AddedDate
        //{
        //    get => DateTime.TryParse(Added, out var date) ? date : (DateTime?)null;
        //    set => Added = value?.ToString(Static.DateTimeFormat);
        //}
    }
}
