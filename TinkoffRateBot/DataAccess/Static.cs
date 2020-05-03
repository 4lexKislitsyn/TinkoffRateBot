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
        public const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";
    }
}
