using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace authenticationlab
{
    public class Startup
    {
        private ILogger _logger;

        public Startup(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(AwfulQueryStringAuthenticationDefaults.AuthenticationScheme)
                .AddAwfulQueryString(options =>
                {
                    options.Events = new AwfulQueryStringAuthenticationEvents
                    {
                        OnValidateUsername = context =>
                        {
                            if (context.Username == "blowdart")
                            {
                                var claims = new[]
                                {
                                    new Claim(
                                        ClaimTypes.Name,
                                        context.Username,
                                        ClaimValueTypes.String,
                                        context.Options.ClaimsIssuer)
                                };

                                context.Principal = new ClaimsPrincipal(
                                    new ClaimsIdentity(claims, context.Scheme.Name));

                                context.Success();
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStatusCodePages();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}