﻿using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinkoffRateBot.DataAccess.Converters;

namespace TinkoffRateBot.DataAccess.Models
{
    [DynamoDBTable(nameof(TinkoffExchangeRate))]
    public class TinkoffExchangeRate
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

        //[DynamoDBIgnore]
        //public DateTime? UpdatedDate
        //{
        //    get => DateTime.TryParse(Updated, out var date) ? date : (DateTime?)null;
        //    set => Updated = value?.ToString(Static.DateTimeFormat);
        //}
    }
}