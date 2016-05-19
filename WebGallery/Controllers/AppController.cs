using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class AppController : Controller
    {
        [AllowAnonymous]
        public async Task<ActionResult> Categorize()
        {
            var model = new AppCategorizeViewModel();

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> Detail(int id)
        {
            var model = new AppDetailViewModel();

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> Mine()
        {
            var model = new AppMineViewModel();

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> Owners(int id)
        {
            var model = new AppOwnersViewModel();

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> Status(int id)
        {
            var model = new AppStatusViewModel();

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> Submit(int id)
        {
            return null;
        }
    }
}