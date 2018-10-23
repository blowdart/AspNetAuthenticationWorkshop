using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace authenticationlab.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var model = new Models.IndexViewModel();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
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
                        model.ProfilePictureUri = thumbnailUrl;
                    }
                }
            }

            return View(model);
        }
    }
}