using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace authenticationlab
{
    public class Startup
    {
        private const string AccessTokenClaim = "urn:tokens:google:accesstoken";

        private ILogger _logger;

        public Startup(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = "**CLIENT ID**";
                    options.ClientSecret = "**CLIENT SECRET**";
                    options.Events = new OAuthEvents
                    {
                        OnRedirectToAuthorizationEndpoint = context =>
                        {
                            _logger.LogInformation("Redirecting to {0}", context.RedirectUri);
                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            _logger.LogInformation("Something went horribly wrong.");
                            return Task.CompletedTask;
                        },
                        OnTicketReceived = context =>
                        {
                            _logger.LogInformation("Ticket received.");
                            return Task.CompletedTask;
                        },
                        OnCreatingTicket = context =>
                        {
                            _logger.LogInformation("Creating tickets.");
                            var identity = (ClaimsIdentity)context.Principal.Identity;
                            identity.AddClaim(new Claim(AccessTokenClaim, context.AccessToken));

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.Run(async (context) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync();
                }
                context.Response.Headers.Add("Content-Type", "text/html");

                await context.Response.WriteAsync("<html><body>\r");

                var claimsIdentity = (ClaimsIdentity)context.User.Identity;
                var nameIdentifier = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var googleApiKey = "**API KEY**";

                if (!string.IsNullOrEmpty(nameIdentifier))
                {
                    var jsonUrl = $"https://www.googleapis.com/plus/v1/people/{nameIdentifier}?fields=image&key={googleApiKey}";
                    using (var httpClient = new HttpClient())
                    {
                        var s = await httpClient.GetStringAsync(jsonUrl);
                        dynamic deserializeObject = JsonConvert.DeserializeObject(s);
                        var thumbnailUrl = (string)deserializeObject.image.url;
                        if (thumbnailUrl != null && !string.IsNullOrWhiteSpace(thumbnailUrl))
                        {
                            await context.Response.WriteAsync(
                                string.Format($"<img src=\"{thumbnailUrl}\"></img>"));
                        }
                    }
                }
                await context.Response.WriteAsync("</body></html>\r");
            });
        }
    }
}