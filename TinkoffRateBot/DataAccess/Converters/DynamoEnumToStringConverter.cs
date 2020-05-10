using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.DataAccess.Converters
{
    /// <summary>
    /// <see cref="Enum"/> to <see cref="string"/> converter.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public class DynamoEnumToStringConverter<TEnum> : IPropertyConverter where TEnum : struct
    {
        /// <inheritdoc/>
        public object FromEntry(DynamoDBEntry entry) 
            => Enum.Parse<TEnum>(entry.AsString());
        /// <inheritdoc/>
        public DynamoDBEntry ToEntry(object value) => new Primitive(value?.ToString());
    }
}
