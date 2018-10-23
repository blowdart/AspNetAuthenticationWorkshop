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

        private static int RequestCount = 0;

        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path == "/")
            {
                System.Threading.Interlocked.Increment(ref RequestCount);
            }

            if (RequestCount % 5 == 0)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
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
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = new System.TimeSpan(0, 5, 0);
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = Startup.ValidateAsync
                    };
                })
                .AddGoogle(options =>
                {
                    options.ClientId = "** CLIENT ID **";
                    options.ClientSecret = "** CLIENT SECRET **";
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
                            var identity = (ClaimsIdentity)context.Principal.Identity;
                            identity.AddClaim(new Claim(AccessTokenClaim, context.AccessToken));
                            _logger.LogInformation("Creating tickets.");
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
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
                var googleApiKey = "** API KEY **";

                if (!string.IsNullOrEmpty(nameIdentifier))
                {
                    string jsonUrl = $"https://www.googleapis.com/plus/v1/people/{nameIdentifier}?fields=image&key={googleApiKey}";
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

                var transformDateClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == "transformedOn");
                if (transformDateClaim != null)
                {
                    await context.Response.WriteAsync(
                        string.Format("<br/>Transformed on {0}.<br />", transformDateClaim.Value));
                }

                await context.Response.WriteAsync("</body></html>\r");
            });
        }
    }

    class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;

            if (claimsIdentity.Claims.FirstOrDefault(x => x.Type == "transformedOn") == null)
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("transformedOn", System.DateTime.Now.ToString()));
            }

            return Task.FromResult(principal);
        }
    }
}