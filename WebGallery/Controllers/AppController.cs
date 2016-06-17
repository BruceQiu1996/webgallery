using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Extensions;
using WebGallery.Filters;
using WebGallery.Models;
using WebGallery.Security;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class AppController : Controller
    {
        private IAppService _appService;
        private IAppValidationService _validationService;
        private ISubmitterService _submitterService;
        private IEmailService _emailService;

        public AppController() : this(new AppService(), new AppValidationService(), new SubmitterService(), new EmailService()) { }

        public AppController(IAppService appService,
            IAppValidationService validationService,
            ISubmitterService submitterService,
            IEmailService emailService)
        {
            _appService = appService;
            _validationService = validationService;
            _submitterService = submitterService;
            _emailService = emailService;
        }

        #region create/edit/clone submission

        [Authorize]
        [HttpGet]
        [RequireSubmittingAppEnabled]
        [RequireSubmittership]
        [RequireBrowserVersion]
        public async Task<ActionResult> New(bool? testMode)
        {
            var model = (testMode.HasValue && testMode.Value)
                        ? AppSubmitViewModel.Fake()
                        : AppSubmitViewModel.Empty();

            await LoadViewDataForSubmit();

            return View("Submit", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittingAppEnabled]
        [RequireSubmittership]
        public async Task<ActionResult> New(AppSubmitViewModel model)
        {
            return await Create(model);
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittingAppEnabled]
        [RequireSubmittership]
        [RequireBrowserVersion]
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
        [RequireSubmittingAppEnabled]
        [RequireSubmittership]
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

            // go to the App Status page
            // old site -> Response.Redirect("AppStatus.aspx?mode=thanks&id=" + id);
            return RedirectToAction("Verify", new { id = submission.SubmissionID, showThanks = true });
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittingAppEnabled]
        [RequireSubmittership]
        [RequireBrowserVersion]
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

            // Check if current user can modify the app.
            // Only the owner and a super submitter can do that.
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId))
            {
                return View("NeedPermission");
            }

            // Check if modification of this submission is locked.
            if (!User.IsSuperSubmitter() && await _appService.IsModificationLockedAsync(submissionId))
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
                CanEditNickname = User.IsSuperSubmitter()
            };

            await LoadViewDataForSubmit();

            return View("Submit", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittingAppEnabled]
        [RequireSubmittership]
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

            // go to the App Status page
            return RedirectToAction("Verify", new { id = submission.SubmissionID, showThanks = true });
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

        [AllowAnonymous]
        public async Task<ActionResult> Gallery(string searchString, int page = 1)
        {
            var model = new AppGalleryViewModel
            {
                TotalPage = Convert.ToInt32(Math.Ceiling((double)(await _appService.GetAppList(searchString)).Count() / 20.0)),
                AppList = (await _appService.GetAppList(searchString)).Skip((page - 1) * 20).Take(20),
                CurrentPage = page,
                Keyword = searchString
            };

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Detail(int id)
        {
            var submision = await _appService.GetSubmissionAsync(id);
            if (submision == null)
            {
                return View("ResourceNotFound");
            }

            var metaData = await _appService.GetMetadataAsync(id);
            if (metaData.Count() == 0)
            {
                return View("NeedAppNameAndDescription", submision.SubmissionID);
            }

            var model = new AppDetailViewModel
            {
                Submission = submision,
                Categories = await _appService.GetCategoriesAsync(),
                MetaData = metaData.FirstOrDefault(p => p.Language == Language.CODE_ENGLISH_US) ?? metaData.FirstOrDefault()
            };

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
        [HttpGet]
        [RequireSubmittership]
        public async Task<ActionResult> Verify(int? id, bool? showThanks)
        {
            if (!id.HasValue) return View("ResourceNotFound");

            var submissionId = id.Value;
            var isOwner = await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId);
            if (!User.IsSuperSubmitter() && !isOwner)
            {
                return View("NeedPermission");
            }

            var submission = await _appService.GetSubmissionAsync(submissionId);
            if (submission == null)
            {
                return View("ResourceNotFound");
            }

            var model = new AppVerifyViewModel
            {
                Submission = submission,
                ValidationItems = await _validationService.GetValidationItemsAsync(submission),
                ShowThanks = showThanks ?? false
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyUrl(string url)
        {
            var status = await _validationService.ValidateUrlAsync(url);

            return Json(new { status = status.ToString() });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPackage(string url, string hash, int submissionId)
        {
            var packageValidationResult = await _validationService.ValidatePackageAsync(url, hash, submissionId);

            return Json(new
            {
                ManifestStatus = packageValidationResult.ManifestStatus.ToString(),
                HashStatus = packageValidationResult.HashStatus.ToString()
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyImage(string url, bool isLogo)
        {
            var imageValidationResult = await _validationService.ValidateImageAsync(url, isLogo);

            return Json(new
            {
                TypeStatus = imageValidationResult.TypeStatus.ToString(),
                DimensionStatus = imageValidationResult.DimensionStatus.ToString()
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> MoveToTesting(int submissionId)
        {
            var submission = await _appService.GetSubmissionAsync(submissionId);
            if (submission == null)
            {
                return View("ResourceNotFound");
            }

            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId))
            {
                return View("NeedPermission");
            }

            await _appService.MoveToTestingAsync(submission);

            // send email
            _emailService.SendMessageForSubmissionVerified(User.GetSubmittership(), submission, HttpContext.Request.Url.Authority, html => { return HttpContext.Server.HtmlEncode(html); });

            return RedirectToAction("mine");
        }
    } // end class
}