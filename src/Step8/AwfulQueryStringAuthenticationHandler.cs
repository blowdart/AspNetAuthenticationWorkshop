using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace authenticationlab
{
    public class AwfulQueryStringAuthenticationHandler : AuthenticationHandler<AwfulQueryStringAuthenticationOptions>
    {
        public AwfulQueryStringAuthenticationHandler(
            IOptionsMonitor<AwfulQueryStringAuthenticationOptions> options,
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
                var validateCredentialsContext = new ValidateCredentialsContext(Context, Scheme, Options)
                {
                    Username = usernameParameter
                };

                await Options.Events.ValidateUsername(validateCredentialsContext);

                if (validateCredentialsContext.Result != null)
                {
                    var ticket = new AuthenticationTicket(validateCredentialsContext.Principal, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
            }

            return AuthenticateResult.NoResult();
        }
    }
}
