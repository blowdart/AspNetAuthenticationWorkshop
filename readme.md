# ASP.NET Core Authentication Lab

https://github.com/dotnet-presentations

This is walk through for an ASP.NET Core Authentication Lab, targetted against ASP.NET Core 2.0 RTM and VS2017/VS Cde.

This lab uses the Model-View-Controller template as that's what everyone has been using up until now and it's the most familiar starting point for the vast majority of people.

Official [authentication documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/) is at https://docs.microsoft.com/en-us/aspnet/core/security/authentication/.

*An [authorization lab](https://github.com/blowdart/AspNetAuthorizationWorkshop/tree/core2) is available at https://github.com/blowdart/AspNetAuthorizationWorkshop/tree/core2*

Step 0: Preparation
===================

Prerequisites
=============
* [Visual Studio 2017](https://www.visualstudio.com/) (Community edition is free) or
* [Visual Studio Code](https://www.visualstudio.com/) (Code is free) and
* [.NET Core 2.0 SDK](https://www.microsoft.com/net/download/).

Create a new, blank, project.
-----------------------------

We're going to start with the command line.

* Create a directory on your computer somewhere, call it authenticationlab
* Open a command line/shell and change to the directory you created
* At the command line type `dotnet new console`
* Once that completes type `dotnet run` and you will see "Hello World"

Let's examine what's been created. The directory contains two files
* `authenticationlab.csproj`
* `Program.cs`

Type `code .` and VS Code will open the directory. It may install some things that are missing. Explore the two files.

* The CS proj file is the project file, it contains the instructions on how the project should be built and what should be included
* `program.cs` contains the instructions to output 'Hello World'.

Let's turn this into a web application. .NET Core is the core libraries and runtime. ASP.NET Core adds support for web applications, including Kestrel a web server.

First let's add ASP.NET Core to the application.

* Edit the `csproj` file and add the following after the closing `PropertyGroup` element

``` xml
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.5" />
  </ItemGroup>
```

* Your `csproj` should now look like this

``` xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.5" />
  </ItemGroup>

</Project>
```

* Save the `csproj` file. If you're using Visual Studio or VS Code you may be prompted to restore packages, choose yes. If you're using the command line and an editor that makes you reconsider your life choices like VIM enter `dotnet restore` at the command line. No, I can't help you exit VIM.
* Now switch your editor to the `program.cs` file. Change the contents of this file to be as follows

```c#
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace authenticationlab
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
```

* Note that `.UseStartup<Startup>()` is having a bad time, it's looking for a class called startup. So let's add that, create a new file called `startup.cs` and paste the following into it

```c#
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace authenticationlab
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
```

* Build the application using your IDE `dotnet build`
* Run the application using your IDE or using `dotnet run`
* Open a browser and browser to http://localhost:5000/
* Marvel at seeing Hello World!
* Congratulations you have a web application in .NET Core.

Step 1: Setup authentication
============================

* Navigate to https://apps.twitter.com/ and sign in. If you don't already have a Twitter account, use the Sign up now link to create one. Create a new application, with a callback address of 	http://localhost:5000/signin-twitter
* Make a note of your API Key and API secret from the Keys and Access Tokens tab.
* Replace `startup.cs` with the following code, putting your application Consumer Key and Secret in the options properties.

```c#
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace authenticationlab
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = TwitterDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTwitter(options =>
                {
                    options.ConsumerKey = "CONSUMER_KEY";
                    options.ConsumerSecret = "CONSUMER_SECRET";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            app.Run(async (context) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync();
                }
                await context.Response.WriteAsync("Hello "+context.User.Identity.Name+"!\r");
            });
        }
    }
}
```

* Run the code, authorize the application against your twitter account, and you should see "Hello *yourTwitterUserName*!"
* There is an XSS attack in the sample if the browser decided the page was HTML, so to address this, add the following before the `WriteAsync` call

```c#
context.Response.Headers.Add("Content-Type", "text/plain");
````


Step 2: Authentication Events and Logging
=========================================

* Let's add some logging to see what's going on. 
* Create a private class level variable of type `ILogger` called `_logger` inside the `Startup` class.
* Add a constructor which takes a parameter of `ILoggerFactory` and creates and assigns a logger to `_logger` . 
* *You will need to add a `using` reference to `Microsoft.Extensions.Logging` if your editor isn't doing that work for you.*
* This should look something like the following;

```c#
        private ILogger _logger;

        public Startup(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
        }
```

* Now let's look at the events on the Twitter authenication service.
* Add some logging inside the events class

```c#
options.Events = new TwitterEvents()
{
    OnRedirectToAuthorizationEndpoint = context =>
    {
        _logger.LogInformation("Redirecting to {0}", context.RedirectUri);
       return Task.CompletedTask;
    },
    OnRemoteFailure = context =>
    {
        _logger.LogInformation("Something went horribly wrong.");
        return Task.CompletedTask;
    },
    OnTicketReceived = context =>
    {
        _logger.LogInformation("Ticket recieved.");
        return Task.CompletedTask;
    },
    OnCreatingTicket = context =>
    {
        _logger.LogInformation("Creating tickets.");
        return Task.CompletedTask;
    }
};
```

* Make sure your authentication cookie is deleted (close your browser, or manually cull it) then browse to the app again and watch the logging in the console.
* Did you notice any difference? Why isn't your username greeting you any more?
* Some events need things returned. Look at the documentation for the events, or the [source](https://github.com/aspnet/Security/blob/dev/src/Microsoft.AspNetCore.Authentication.Twitter/Events/TwitterEvents.cs).
* Note that the `OnRedirectToAuthorizationEndpoint` default implementation calls the redirect - this isn't happening in your code any more, so add it back;

```c#
OnRedirectToAuthorizationEndpoint = context =>
{
    _logger.LogInformation("Redirecting to {0}", context.RedirectUri);
    context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
},
```

* Now, take a look at the context properties inside `OnCreatingTicket()`. There's some useful stuff in there, like the Twitter access token. What if I want to save that?
* Let's store the Twitter access token and secret in the identity we're creating from Twitter inside `OnCreatingTicket` as claims.
* First we need names for the claims, so define some `const` values in the `Startup` class like so

```c#
private const string AccessTokenClaim = "urn:tokens:twitter:accesstoken";
private const string AccessTokenSecret = "urn:tokens:twitter:accesstokensecret";
```

* Now inside the `OnCreatingTicket` event let's use these names to create some new claims, with the appropriate values

```c#
OnCreatingTicket = context =>
{
    _logger.LogInformation("Creating tickets.");
    var identity = (ClaimsIdentity)context.Principal.Identity;
    identity.AddClaim(new Claim(AccessTokenClaim, context.AccessToken));
    identity.AddClaim(new Claim(AccessTokenSecret, context.AccessTokenSecret));
    return Task.CompletedTask;
}
```

* And finally to check they persisted add some code inside the `app.Run()` lamda after the greeting;

```c#
var claimsIdentity = (ClaimsIdentity)context.User.Identity;
var accessTokenClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == AccessTokenClaim);
var accessTokenSecretClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == AccessTokenSecret);

if (accessTokenClaim != null && accessTokenSecretClaim != null)
{
    await context.Response.WriteAsync("Twitter access claims have persisted\r");
}
```

* Make sure your cookies have been cleared, run the app and browse to it.
* How safe is this? Can you figure out from the cookie what the access token details are?
* Let's do something interesting with these tokens. 
* The [TweetinviAPI](https://github.com/linvi/tweetinvi) library provides a nice wrapper around the Twitter API. 
* Open up your project file and add a project reference to it under the existing reference for `Microsoft.AspNet.Core.All`.

```xml
<PackageReference Include="TweetinviAPI" Version="2.1.0" />
```

* Remember if you're not using VS you will need to run `dotnet restore` at the command line.
* Now let's use the TweetSharp library to grab the profile image for the authenticated user. 
* Replace the `app.Run()` lamda with the following;

```c#
app.Run(async (context) =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        await context.ChallengeAsync();
    }
    context.Response.Headers.Add("Content-Type", "text/html");

    await context.Response.WriteAsync("<html><body>\r");

    var claimsIdentity = (ClaimsIdentity)context.User.Identity;
    var accessTokenClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == AccessTokenClaim);
    var accessTokenSecretClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == AccessTokenSecret);

    if (accessTokenClaim != null && accessTokenSecretClaim != null)
    {
        var userCredentials = Tweetinvi.Auth.CreateCredentials(
            "CONSUMER_KEY",
            "CONSUMER_SECRET",
            accessTokenClaim.Value,
            accessTokenSecretClaim.Value);

            var authenticatedUser = Tweetinvi.User.GetAuthenticatedUser(userCredentials);
            if (authenticatedUser != null && !string.IsNullOrWhiteSpace(authenticatedUser.ProfileImageUrlHttps))
            {
                await context.Response.WriteAsync(
                    string.Format("<img src=\"{0}\"></img>", authenticatedUser.ProfileImageUrlHttps));
            }
    }

    await context.Response.WriteAsync("</body></html>\r");
});
```
* Rerun the application and look at how wonderful your Twitter profile image is.

Step 4: Cookie Authentication
=============================

* What's the expiry on the issued cookie?
* How are cookies protected?
* Discuss data protection

Step 5: Claims transformation
=============================

Step 6: MVC and Tag Helpers
===========================

CSRF

Step 7: ASP.NET Identity
========================

Step 8: Writing your own authentication provider
================================================


