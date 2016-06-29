using PagedList;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Models;
using WebGallery.Security;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class ManageController : Controller
    {
        private IAppService _appService;
        public ManageController() : this(new AppService()) { }
        public ManageController(IAppService appSerivce)
        {
            _appService = appSerivce;
        }

        //GET 
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Dashboard(string keyword, int? page, int? pageSize, string sortOrder)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToAction("mine", "app");
            }

            var count = 0;
            var pageNumber = page.HasValue ? page.Value : 1;
            var size = pageSize.HasValue ? pageSize.Value : 10;
            var apps = await _appService.GetSubmissionsAsync(keyword, pageNumber, size, sortOrder, out count);
            var model = new ManageDashboardViewModel
            {
                PageSize = size,
                Keyword = keyword,
                CurrentSort = sortOrder,
                Submissions = new StaticPagedList<Submission>(apps, pageNumber, size, count),
                Status = await _appService.GetStatusAsync()
            };

            return View("Dashboard", model);
        }

        public async Task<ActionResult> SuperSubmitters()
        {
            var model = new ManageSuperSubmittersViewModel();

            return View(model);
        }
    }
}