using System;
using Microsoft.AspNetCore.Authentication;

using authenticationlab;


namespace Microsoft.AspNetCore.Builder
{
    public static class AwfulQueryStringAuthenticationAppBuilderExtensions
    {
        public static AuthenticationBuilder AddAwfulQueryString(this AuthenticationBuilder builder)
            => builder.AddAwfulQueryString(AwfulQueryStringAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddAwfulQueryString(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddAwfulQueryString(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddAwfulQueryString(this AuthenticationBuilder builder, Action<AwfulQueryStringAuthenticationOptions> configureOptions)
            => builder.AddAwfulQueryString(AwfulQueryStringAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddAwfulQueryString(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<AwfulQueryStringAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<AwfulQueryStringAuthenticationOptions, AwfulQueryStringAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}