using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace authenticationlab
{
    public class AwfulQueryStringAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AwfulQueryStringAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string usernameParameter = Request.Query["username"];

            if (!string.IsNullOrEmpty(usernameParameter))
            {
                var identity = new ClaimsIdentity(Scheme.Name);
                identity.AddClaim(
                    new Claim(
                        ClaimTypes.Name,
                        usernameParameter,
                        ClaimValueTypes.String,
                        Options.ClaimsIssuer));
                var principal = new ClaimsPrincipal(identity);

                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.NoResult();
        }
    }
}
