using System.Web.Mvc;
using WebGallery.ViewModels;

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
    }
}