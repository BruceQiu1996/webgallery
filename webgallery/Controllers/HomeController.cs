using System;
using System.Web;
using System.Web.Mvc;

namespace WebGallery.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Documents()
        {
            return View();
        }

        public ActionResult Agreement()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LanguageSelect(string languageCode, string returnUrl)
        {
            var cookie = new HttpCookie("LanguagePreference");
            cookie.Value = languageCode;
            cookie.Expires = DateTime.MaxValue;
            if (Request.Cookies["LanguagePreference"] == null)
            {
                Response.Cookies.Add(cookie);
            }
            else
            {
                Response.Cookies.Set(cookie);
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
                return RedirectToRoute(SiteRouteNames.Home);
            else
                return Redirect(returnUrl);
        }
    }
}