using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebGallery.Models;
using WebGallery.Security;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class AccountController : Controller
    {
        private ISubmitterService _submitterService;

        public AccountController() : this(new SubmitterService()) { }

        public AccountController(ISubmitterService submitterService)
        {
            _submitterService = submitterService;
        }

        public void SignIn()
        {
            // Send an OpenID Connect sign-in request.
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/app/mine" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        [Authorize]
        // BUGBUG: Ending a session with the v2.0 endpoint is not yet supported.  Here, we just end the session with the web app.
        public void SignOut()
        {
            // Send an OpenID Connect sign-out request.
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            Response.Redirect("/");
        }

        [Authorize]
        [ActionName("Profile")]
        public async Task<ActionResult> Me()
        {
            var model = new AccountProfileViewModel();

            //

            return View(model);
        }

        [Authorize]
        [ActionName("Profile")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Me(AccountProfileViewModel model)
        {

            Submitter submitter = null; // get the submitter
            HttpContext.GetOwinContext().Authentication.SignInAsSubmitter(submitter);

            return View();
        }
    }
}