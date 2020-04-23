using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.DataAccess.Converters
{
    public class DynamoEnumToStringConverter<TEnum> : IPropertyConverter where TEnum : struct
    {
        public object FromEntry(DynamoDBEntry entry) 
            => Enum.Parse<TEnum>(entry.AsString());

        public DynamoDBEntry ToEntry(object value) => new Primitive(value?.ToString());
    }
}
