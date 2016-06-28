using System.Collections.Generic;
using System.Linq;
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
        private IManageService _manageService;
        public ManageController() : this(new ManageService()) { }
        public ManageController(IManageService manageSerivce)
        {
            _manageService = manageSerivce;
        }
        //
        // GET: /Manage/Index
        public ActionResult Index()
        {
            ViewBag.Name = User.GetName();
            ViewBag.PreferredUsername = User.GetPreferredUsername();
            ViewBag.EmailAddress = User.GetEmailAddress();

            return View();
        }
        //GET 
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Dashboard(string sortOrder, string keyword, int? pageSize, int? page)
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
                Submissions = await _manageService.GetSubmissionsInBrief(sortOrder, keyword, pageSize, page),
                Status = await _manageService.GetAllStatus()
            };

            return View("Dashboard", model);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Dashboard(string sortOrder, string keyword, int submissionId, string newStatus, int? pageSize, int? page)
        {
            await _manageService.ChangeSubmissionStatus(submissionId, newStatus);

            return RedirectToAction("dashboard", new { sortOrder = sortOrder, keyword = keyword, pageSize = pageSize, page = page });
        }

        public ActionResult Delete(int id)
        {
            using (var db = new WebGalleryDbContext())
            {
                IEnumerable<Submission> submissions = db.Submissions.ToList<Submission>();

                Submission selectedsubmission = new Submission();
                foreach (Submission submission in submissions)
                {
                    if (submission.SubmissionID == id)
                        selectedsubmission = submission;
                    break;

                }
                // Delete a submission

                return View("AppSubmit", submissions);
            }
        }

        public async Task<ActionResult> SuperSubmitters()
        {
            var model = new ManageSuperSubmittersViewModel();

            return View(model);
        }
    }
}