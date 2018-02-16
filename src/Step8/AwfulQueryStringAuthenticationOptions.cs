using Microsoft.AspNetCore.Authentication;

namespace authenticationlab
{
    public class AwfulQueryStringAuthenticationOptions : AuthenticationSchemeOptions
    {

        public AwfulQueryStringAuthenticationOptions()
        {
        }

        public new AwfulQueryStringAuthenticationEvents Events { get; set; } = new AwfulQueryStringAuthenticationEvents();
    }
}
