using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
                    new AuthenticationProperties { RedirectUri = "/account/submittership" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        [Authorize]
        public async Task<ActionResult> Submittership()
        {
            var authenticationManager = HttpContext.GetOwinContext().Authentication;
            var userIdentity = authenticationManager.User.Identity as ClaimsIdentity;
            if (userIdentity != null)
            {
                var emailAddress = authenticationManager.User.GetEmailAddress();
                var submitter = await _submitterService.GetSubmitterByMicrosoftAccountAsync(emailAddress);
                if (submitter != null)
                {
                    SubmitterClaims.AddSubmitterClaims(userIdentity, submitter);
                    authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, userIdentity);
                }
            }

            return RedirectToAction("Index", "Manage");
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
            return View();
        }
    }
}