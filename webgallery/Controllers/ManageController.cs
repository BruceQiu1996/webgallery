using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Models;
using WebGallery.Security;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private IAppService _appService;
        public ManageController() : this(new AppService()) { }
        public ManageController(IAppService appSerivce)
        {
            _appService = appSerivce;
        }

        //GET 
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Dashboard(string keyword, int? page, int? pageSize, string sortOrder)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToAction("mine", "app");
            }

            var model = new ManageDashboardViewModel
            {
                PageSize = pageSize.HasValue ? pageSize.Value.ToString() : "10",
                Keyword = keyword,
                CurrentSort = sortOrder,
                Submissions = await _appService.GetSubmissions(keyword, page, pageSize, sortOrder),
                Status = await _appService.GetAllStatus()
            };

            return View("Dashboard", model);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Dashboard(string keyword, int? page, int? pageSize, string sortOrder, int submissionId, int statusId)
        {
            await _appService.UpdateSubmissionStatus(submissionId, statusId);

            return RedirectToAction("dashboard", new { keyword = keyword, page = page, pageSize = pageSize, sortOrder = sortOrder });
        }

        public async Task<ActionResult> SuperSubmitters()
        {
            var model = new ManageSuperSubmittersViewModel();

            return View(model);
        }
    }
}