using System.Threading.Tasks;
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
            var model = new HomeAgreementViewModel();

            return View(model);
        }

        public ActionResult Error(string message)
        {
            return View(message);
        }
    }
}