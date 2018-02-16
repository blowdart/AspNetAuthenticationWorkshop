using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace authenticationlab
{
    public class ValidateCredentialsContext : ResultContext<AwfulQueryStringAuthenticationOptions>
    {
        public ValidateCredentialsContext(
            HttpContext context,
            AuthenticationScheme scheme,
            AwfulQueryStringAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        public string Username { get; set; }
    }
}
