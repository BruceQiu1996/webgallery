using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
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

            var submitter = await _submitterService.GetSubmitterByMicrosoftAccountAsync(User.GetEmailAddress());
            if (submitter != null)
            {
                model.ContactDetail = await _submitterService.GetContactDetailAsync(submitter.SubmitterID);
            }

            return View("Profile", model);
        }

        [Authorize]
        [ActionName("Profile")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Me(AccountProfileViewModel model)
        {
            if (!ValidateContactDetail(model)) return View(model);

            var submitter = await _submitterService.SaveContactDetailAsync(User.GetEmailAddress(), model.ContactDetail);

            // add submitter claims
            HttpContext.GetOwinContext().Authentication.SignInAsSubmitter(submitter);

            return RedirectToAction("Profile");
        }

        private bool ValidateContactDetail(AccountProfileViewModel model)
        {
            Requires(model.ContactDetail.FirstName, "ContactDetail.FirstName", "*");
            Requires(model.ContactDetail.LastName, "ContactDetail.LastName", "*");
            Requires(model.ContactDetail.City, "ContactDetail.City", "*");
            Requires(model.ContactDetail.Address1, "ContactDetail.Address1", "*");
            Requires(model.ContactDetail.EMail, "ContactDetail.EMail", "*");

            if (model.ContactDetail.Country == "0")
            {
                ModelState.AddModelError("ContactDetail.Country", "*");
            }

            if (model.ContactDetail.Country == "USA" && string.IsNullOrWhiteSpace(model.ContactDetail.ZipOrRegionCode))
            {
                ModelState.AddModelError("ContactDetail.ZipOrRegionCode", "*");
            }

            if (model.ContactDetail.Country == "USA" && string.IsNullOrWhiteSpace(model.ContactDetail.StateOrProvince))
            {
                ModelState.AddModelError("State", "*");
            }

            return ModelState.IsValid;
        }

        private void Requires(string element, string field, string message)
        {
            if (string.IsNullOrWhiteSpace(element))
            {
                ModelState.AddModelError(field, message);
            }
        }
    }
}