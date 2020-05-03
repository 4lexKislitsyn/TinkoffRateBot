using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TinkoffRateBot
{
    internal class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly BasicAuthHandlerConfiguration _handlerOptions;

        public BasicAuthHandler(IOptions<BasicAuthHandlerConfiguration> handlerOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _handlerOptions = handlerOptions.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var authHeader = Request.Headers[HeaderNames.Authorization];
                if (string.IsNullOrWhiteSpace(authHeader))
                {
                    return Task.FromResult(AuthenticateResult.Fail("Authorization header is empty."));
                }
                var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);

                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty)).Split(':', 2);

                var result = credentials.Length == 2 && _handlerOptions.Users.Any(x=> credentials[0] == x.Name && credentials[1] == x.Password)
                    ? AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(), AuthenticationSchemes.Basic.ToString()))
                    : AuthenticateResult.Fail("No user with equals credentials");
                return Task.FromResult(result);

            }
            catch (FormatException ex)
            {
                return Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }
    }
}