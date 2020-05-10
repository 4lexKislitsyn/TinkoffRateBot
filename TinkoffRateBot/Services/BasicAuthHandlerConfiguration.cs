using System.Collections.Generic;
using Telegram.Bot.Types;
using TinkoffRateBot.Services;

namespace TinkoffRateBot
{
    /// <summary>
    /// Configuration for <see cref="BasicAuthHandler"/>.
    /// </summary>
    public partial class BasicAuthHandlerConfiguration
    {
        /// <summary>
        /// Active user credentials.
        /// </summary>
        public IEnumerable<BasicAuthUser> Users { get; set; }
    }
}