using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Extensions;
using WebGallery.Models;
using WebGallery.Security;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class AppController : Controller
    {
        private IAppService _appService;
        private ISubmitterService _submitterService;
        private IEmailService _emailService;

        public AppController() : this(new AppService(), new SubmitterService(), new EmailService()) { }

        public AppController(IAppService appService, ISubmitterService submitterService, IEmailService emailService)
        {
            _appService = appService;
            _submitterService = submitterService;
            _emailService = emailService;
        }

        #region create/edit/clone submission

        [Authorize]
        public async Task<ActionResult> New(bool? testMode)
        {
            // do common checks before submitting an app
            var failureResult = await Precheck();
            if (failureResult != null)
            {
                return failureResult;
            }

            var model = (testMode.HasValue && testMode.Value)
                        ? AppSubmitViewModel.Fake()
                        : AppSubmitViewModel.Empty();

            await LoadViewDataForSubmit();

            return View("Submit", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> New(AppSubmitViewModel model)
        {
            return await Create(model);
        }

        [Authorize]
        public async Task<ActionResult> Clone(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("New");
            }

            int submissionId = id.Value;
            var submission = await _appService.GetSubmissionAsync(submissionId);

            // Per the rule in the old site, 
            // if the submission id doesn't refer to a submission in database, 
            // the page will show an empty form and allow user to submit a new app.
            if (submission == null)
            {
                return RedirectToAction("New");
            }

            var failureResult = await Precheck();
            if (failureResult != null)
            {
                return failureResult;
            }

            // Check if current user can clone the app specified by the submission id.
            // Only the owner and a super submitter can do that.
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId))
            {
                return View("NeedPermission");
            }

            var model = AppSubmitViewModel.Clone(submission,
                    await _appService.GetMetadataAsync(submissionId),
                    await _appService.GetPackagesAsync(submissionId)
            );

            await LoadViewDataForSubmit();

            return View("Submit", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Clone(AppSubmitViewModel model)
        {
            return await Create(model);
        }

        private async Task<ActionResult> Create(AppSubmitViewModel model)
        {
            // final check
            var finalCheck = _appService.ValidateAppIdCharacters(model.Submission.Nickname)
                                && await _appService.ValidateAppIdVersionIsUniqueAsync(model.Submission.Nickname, model.Submission.Version, model.Submission.SubmissionID);
            if (!finalCheck)
            {
                await LoadViewDataForSubmit();
                ModelState.AddModelError("AppId", "unique");
                return View("Submit", model);
            }

            // save
            var submission = await _appService.CreateAsync(User.GetSubmittership(), model.Submission, model.MetadataList, model.Packages, Request.Files.GetAppImages(), model.GetSettingStatusOfImages(), new AppImageAzureStorageService());

            // send email
            // old site -> AppSubmissionEMailer.SendAppSubmissionMessage(id, ID > 0);
            _emailService.SendAppSubmissionMessage(User.GetSubmittership(), submission, true);

            // go to the App Status page
            // old site -> Response.Redirect("AppStatus.aspx?mode=thanks&id=" + id);
            return RedirectToAction("Status", new { id = submission.SubmissionID });
        }

        [Authorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("New");
            }

            int submissionId = id.Value;
            var submission = await _appService.GetSubmissionAsync(submissionId);

            // Per the rule in the old site, 
            // if the submission id doesn't refer to a submission in database, 
            // the page will show an empty form and allow user to submit a new app.
            if (submission == null)
            {
                return RedirectToAction("New");
            }

            // do common check before editing an app submission
            var failureResult = await Precheck();
            if (failureResult != null)
            {
                return failureResult;
            }

            // Check if current user can modify the app.
            // Only the owner and a super submitter can do that.
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId))
            {
                return View("NeedPermission");
            }

            // Check if modification of this submission is locked.
            if (await _appService.IsModificationLockedAsync(submissionId))
            {
                // old: disable the form and display "This submission is being reviewed and processed by Microsoft Corp. No modifications can be made at this time."
                // new: go to a warning page
                return View("ModificationLocked");
            }

            var model = new AppSubmitViewModel
            {
                Submission = submission,
                MetadataList = await _appService.GetMetadataAsync(submissionId),
                Packages = await _appService.GetPackagesAsync(submissionId),
                CanEditNickname = false
            };

            await LoadViewDataForSubmit();

            return View("Submit", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AppSubmitViewModel model)
        {
            // Check if current user can modify the app.
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, model.Submission.SubmissionID))
            {
                return View("NeedPermission");
            }

            // final check
            var finalCheck = _appService.ValidateAppIdCharacters(model.Submission.Nickname)
                                && await _appService.ValidateAppIdVersionIsUniqueAsync(model.Submission.Nickname, model.Submission.Version, model.Submission.SubmissionID);
            if (!finalCheck)
            {
                await LoadViewDataForSubmit();
                ModelState.AddModelError("AppId", "unique");
                return View("Submit", model);
            }

            // save
            var submission = await _appService.UpdateAsync(User.GetSubmittership(), model.Submission, model.MetadataList, model.Packages, Request.Files.GetAppImages(), model.GetSettingStatusOfImages(), new AppImageAzureStorageService());

            //
            // send email
            // old site -> AppSubmissionEMailer.SendAppSubmissionMessage(id, ID > 0);

            // go to the App Status page
            // old site -> Response.Redirect("AppStatus.aspx?mode=thanks&id=" + id);
            return RedirectToAction("Status", new { id = submission.SubmissionID });
        }

        private async Task<ActionResult> Precheck()
        {
            // Check if the browser is IE and its version is less than 10.
            // If yes, then the user will be prompted to upgrade IE because the page uses placeholder (a HTML 5 attribute) to show watermark text.
            // See http://www.w3schools.com/tags/att_input_placeholder.asp.
            if (Request.Browser.IsInternetExplorer() && Request.Browser.MajorVersion < 10)
            {
                return View("UpgradeIE", HttpContext.GetGlobalResourceObject("Submit", "UpgradeIE"));
            }

            // Check if submitting app is enabled.
            if (ConfigurationManager.AppSettings["EnableSubmitApp"].ToLower() == "false")
            {
                return View("SubmittingDisabled");
            }

            // If the user is currently not a submtter, then go to account/profile.
            if (!User.IsSubmitter())
            {
                return RedirectToAction("Profile", "Account");
            }

            // If current user is not Super Submitter, and there haven't recorded his/her contact info in this system,
            // then go to account/profile.
            if (!User.IsSuperSubmitter() && !await _submitterService.HasContactInfoAsync(User.GetSubmittership().SubmitterID))
            {
                return RedirectToAction("Profile", "Account");
            }

            return null;
        }

        private async Task LoadViewDataForSubmit()
        {
            ViewBag.Languages = await _appService.GetSupportedLanguagesAsync();
            ViewBag.Categories = await _appService.GetCategoriesAsync();
            ViewBag.Frameworks = await _appService.GetFrameworksAsync();
            ViewBag.DatabaseServers = await _appService.GetDbServersAsync();
            ViewBag.WebServerExtensions = await _appService.GetWebServerExtensionsAsync();
        }

        public JsonResult ValidateAppIdVersion(string appId, string version, int? submissionId)
        {
            lock (_UniqueAppIdValidationLock)
            {
                return Json(_appService.ValidateAppIdVersionIsUniqueAsync(appId, version, submissionId));
            }
        }

        static private string _UniqueAppIdValidationLock = "This is used to lock";

        #endregion

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

    } // end class
}