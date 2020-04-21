using Amazon.DynamoDBv2;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TinkoffRateBot.DataAccess.Models;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

namespace TinkoffRateBot.DataAccess
{
    public static class Static
    {
        public const string DateTimeFormat = "dd/MM/yyyy HH:mm:ss";

        public static async Task InitializeDB(IServiceProvider applicationServices)
        {
            var client = applicationServices.GetRequiredService<IAmazonDynamoDB>();
            var tables = await client.ListTablesAsync();
            if (!tables.TableNames.Contains(nameof(TelegramChatInfo)))
            {
                var createRequest = CreateTableRequest<TelegramChatInfo>();
                await client.CreateTableAsync(createRequest);
            }

            if (!tables.TableNames.Contains(nameof(TinkoffExchangeRate)))
            {
                var createRequest = CreateTableRequest<TinkoffExchangeRate>();
                await client.CreateTableAsync(createRequest);
            }
        }

        private static IEnumerable<KeySchemaElement> GetKeys<T>()
        {
            var properties = typeof(T).GetProperties();

            var hashKey = properties.FirstOrDefault(x => Attribute.IsDefined(x, typeof(DynamoDBHashKeyAttribute)));

            yield return new KeySchemaElement(hashKey.Name, "HASH");

            foreach (var prop in properties.Where(x => Attribute.IsDefined(x, typeof(DynamoDBRangeKeyAttribute))))
            {
                yield return new KeySchemaElement(prop.Name, "RANGE");
            }
        }

        private static IEnumerable<AttributeDefinition> GetAttributes<T>()
        {
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties.Where(x => Attribute.IsDefined(x, typeof(DynamoDBHashKeyAttribute)) || Attribute.IsDefined(x, typeof(DynamoDBRangeKeyAttribute))))
            {
                string type;
                if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
                {
                    type = "S";
                }
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(float))
                {
                    type = "N";
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    type = "N";
                }
                else
                {
                    type = "B";
                }
                yield return new AttributeDefinition(prop.Name, type);
            }
        }

        private static CreateTableRequest CreateTableRequest<T>() => new CreateTableRequest(typeof(T).Name, GetKeys<T>().ToList(), GetAttributes<T>().ToList(), new ProvisionedThroughput
        {
            ReadCapacityUnits = 5,
            WriteCapacityUnits = 5
        });
    }
}
