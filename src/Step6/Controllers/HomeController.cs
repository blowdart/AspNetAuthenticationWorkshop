using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace authenticationlab.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            var model = new Models.IndexViewModel();

            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var accessTokenClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == Startup.AccessTokenClaim);
            var accessTokenSecretClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == Startup.AccessTokenSecret);

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
                    model.ProfilePictureUri = authenticatedUser.ProfileImageUrlHttps;
                }
            }

            return View(model);
        }
    }
}