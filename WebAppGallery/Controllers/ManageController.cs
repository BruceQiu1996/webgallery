using NLog;
using PagedList;
using System;
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
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IAppService _appService;
        private ISubmitterService _submitterService;
        private IEmailService _emailService;
        public ManageController() : this(new AppService(), new SubmitterService(), new EmailService()) { }
        public ManageController(IAppService appSerivce, ISubmitterService submitterService, IEmailService emailService)
        {
            _appService = appSerivce;
            _submitterService = submitterService;
            _emailService = emailService;
        }

        //GET 
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Dashboard(string keyword, int? page, int? pageSize, string sortOrder)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            var defaultPageSize = 20;
            pageSize = pageSize ?? defaultPageSize;
            page = page ?? 1;

            var count = 0;
            var apps = await _appService.GetSubmissionsAsync(keyword, page.Value, pageSize.Value, sortOrder, out count);
            var model = new ManageDashboardViewModel
            {
                PageSize = pageSize.Value,
                Keyword = keyword,
                CurrentSort = sortOrder,
                Submissions = new StaticPagedList<Submission>(apps, page.Value, pageSize.Value, count),
                StatusList = await _appService.GetStatusAsync()
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> SuperSubmitters()
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            var model = new ManageSuperSubmittersViewModel
            {
                SuperSubmitters = await _submitterService.GetSuperSubmittersAsync()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveSuperSubmitter(int submitterId)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            await _submitterService.RemoveSuperSubmitterAsync(submitterId);

            return RedirectToRoute(SiteRouteNames.Supersubmitter);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSuperSubmitter(string microsoftAccount, string firstName, string lastName)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            await _submitterService.AddSuperSubmitterAsync(microsoftAccount, firstName, lastName);

            return RedirectToRoute(SiteRouteNames.Supersubmitter);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAppsInFeed(string keyword, int? page, int? pageSize, string sortOrder)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            var defaultPageSize = 20;
            pageSize = pageSize ?? defaultPageSize;
            page = page ?? 1;

            var count = 0;
            var apps = await _appService.GetAppsFromFeedAsync(keyword, "all", "all", Language.CODE_ENGLISH_US, page.Value, pageSize.Value, sortOrder, out count);
            var model = new ManageDashboardViewModel
            {
                PageSize = pageSize.Value,
                Keyword = keyword,
                CurrentSort = sortOrder,
                Submissions = new StaticPagedList<Submission>(apps, page.Value, pageSize.Value, count)
            };

            return View("AppsInFeed", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RebrandApp(string appId, string newAppId, string returnUrl)
        {
            if (!User.IsSuperSubmitter())
            {
                return View("NeedPermission");
            }

            try
            {
                _logger.Info($"{User.GetEmailAddress()} is rebranding the app {appId} with the new Id {newAppId} ...");
                await _appService.RebrandAsync(appId, newAppId);
                _logger.Info($"{User.GetEmailAddress()} is rebranding the app {appId} with the new Id {newAppId} ... done.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw ex;
            }

            try
            {
                _logger.Info($"Sending message for rebranding ...");
                await _emailService.SendMessageForRebrand(appId, newAppId, User.GetEmailAddress());
                _logger.Info($"Sending message for rebranding ... done.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurs in sending message for rebranding.");
            }

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToRoute(SiteRouteNames.App_Feed);
            else
                return Redirect(returnUrl);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAppFromFeed(string appId, string[] submissionIds, string returnUrl)
        {
            if (!User.IsSuperSubmitter())
            {
                return View("NeedPermission");
            }

            if (!await _appService.IsNewAppAsync(appId))
            {
                await _appService.DeleteAppFromFeedAsync(appId);
            }

            if (submissionIds != null && submissionIds.Length > 0)
            {
                await _appService.DeleteAsync(submissionIds);
            }

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToRoute(SiteRouteNames.App_Feed);
            else
                return Redirect(returnUrl);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetSubmissions(string appId)
        {
            if (!User.IsSuperSubmitter())
            {
                return View("NeedPermission");
            }

            return PartialView("AppsInFeed_Submissions_Partial", await _appService.GetSubmissionsByAppIdAsync(appId));
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Submitters(string keyword, int? page, int? pageSize)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            var defaultPageSize = 20;
            pageSize = pageSize ?? defaultPageSize;
            page = page ?? 1;

            var count = 0;
            var submitters = await _submitterService.GetSubmittersAsync(keyword, page.Value, pageSize.Value, out count);
            var model = new ManageSubmittersViewModel
            {
                PageSize = pageSize.Value,
                Keyword = keyword,
                Submitters = new StaticPagedList<SubmittersContactDetail>(submitters, page.Value, pageSize.Value, count)
            };

            return View("Submitters", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateMsa(int submitterId, string microsoftAccount, string returnUrl)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            await _submitterService.UpdateMsaAsync(submitterId, microsoftAccount);

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToRoute(SiteRouteNames.Submitter);
            else
                return Redirect(returnUrl);
        }
    }
}