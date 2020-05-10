using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.DataAccess.Converters
{
    /// <summary>
    /// Converter for <see cref="DateTime"/> structure.
    /// </summary>
    public class DateTimePropertyConverter : IPropertyConverter
    {
        /// <inheritdoc/>
        public object FromEntry(DynamoDBEntry entry)
        {
            return DateTime.TryParse(entry.AsString(), out var dateTime) ? dateTime : (object)null;
        }
        /// <inheritdoc/>
        public DynamoDBEntry ToEntry(object value)
        {
            if (value is DateTime date)
            {
                return new Primitive(date.ToString(Static.DateTimeFormat));
            }
            throw new ArgumentOutOfRangeException();
        }
    }
}
