using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public async Task<ActionResult> Clone(int? submissionId)
        {
            if (!submissionId.HasValue)
            {
                return RedirectToRoute(SiteRouteNames.App_Submit);
            }

            var submission = await _appService.GetSubmissionAsync(submissionId.Value);

            // Per the rule in the old site, 
            // if the submission id doesn't refer to a submission in database, 
            // the page will show an empty form and allow user to submit a new app.
            if (submission == null)
            {
                return RedirectToRoute(SiteRouteNames.App_Submit);
            }

            // Check if current user can clone the app specified by the submission id.
            // Only the owner and a super submitter can do that.
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submission.SubmissionID))
            {
                return View("NeedPermission");
            }

            var model = AppSubmitViewModel.Clone(submission,
                    await _appService.GetMetadataAsync(submission.SubmissionID),
                    await _appService.GetPackagesAsync(submission.SubmissionID)
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
            // check if the appId is accepted to be submitted
            if (!await _appService.ValidateAppIdAcceptedAsync(model.Submission.Nickname))
            {
                return View("AppIdNotAccepted");
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
            var submission = await _appService.CreateAsync(User.GetSubmittership(), model.Submission, model.MetadataList, model.Packages, Request.Files.GetAppImages(), model.GetSettingStatusOfImages(), new AppImageAzureStorageService());

            // go to the App Status page
            // old site -> Response.Redirect("AppStatus.aspx?mode=thanks&id=" + id);
            return RedirectToRoute(SiteRouteNames.App_Verify, new { submissionId = submission.SubmissionID, showThanks = true });
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittingAppEnabled]
        [RequireSubmittership]
        [RequireBrowserVersion]
        public async Task<ActionResult> Edit(int? submissionId)
        {
            if (!submissionId.HasValue)
            {
                return RedirectToRoute(SiteRouteNames.App_Submit);
            }

            var submission = await _appService.GetSubmissionAsync(submissionId.Value);

            // Per the rule in the old site, 
            // if the submission id doesn't refer to a submission in database, 
            // the page will show an empty form and allow user to submit a new app.
            if (submission == null)
            {
                return RedirectToRoute(SiteRouteNames.App_Submit);
            }

            // Check if current user can modify the app.
            // Only the owner and a super submitter can do that.
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submission.SubmissionID))
            {
                return View("NeedPermission");
            }

            // Check if modification of this submission is locked.
            if (!User.IsSuperSubmitter() && await _appService.IsModificationLockedAsync(submission.SubmissionID))
            {
                // old: disable the form and display "This submission is being reviewed and processed by Microsoft Corp. No modifications can be made at this time."
                // new: go to a warning page
                return View("ModificationLocked");
            }

            var model = new AppSubmitViewModel
            {
                Submission = submission,
                MetadataList = await _appService.GetMetadataAsync(submission.SubmissionID),
                Packages = await _appService.GetPackagesAsync(submission.SubmissionID),
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

            // check if the appId is accepted to be submitted
            if (!await _appService.ValidateAppIdAcceptedAsync(model.Submission.Nickname))
            {
                return View("AppIdNotAccepted");
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

            // go to App Verify
            return RedirectToRoute(SiteRouteNames.App_Verify, new { submissionId = submission.SubmissionID, showThanks = true });
        }

        private async Task LoadViewDataForSubmit()
        {
            ViewBag.Languages = await _appService.GetSupportedLanguagesAsync();
            ViewBag.Categories = await _appService.GetCategoriesAsync();
            ViewBag.Frameworks = await _appService.GetFrameworksAsync();
            ViewBag.DatabaseServers = await _appService.GetDbServersAsync();
            ViewBag.WebServerExtensions = await _appService.GetWebServerExtensionsAsync();
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> ValidateAppIdVersion(string appId, string version, int? submissionId)
        {
            return Json(await _appService.ValidateAppIdVersionIsUniqueAsync(appId, version, submissionId));
        }

        #endregion

        [AllowAnonymous]
        public async Task<ActionResult> Categorize(string category, string supportedLanguage, int? page)
        {
            var count = 0;
            var pageNumber = page ?? 1;
            var pageSize = 20;
            var preferredLanguage = Thread.CurrentThread.GetLanguageCode();
            category = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim();
            supportedLanguage = string.IsNullOrWhiteSpace(supportedLanguage) ? Language.CODE_ENGLISH_US : supportedLanguage;

            var model = new AppCategorizeViewModel
            {
                Submissions = await _appService.GetAppsFromFeedAsync(string.Empty, category, supportedLanguage, preferredLanguage, pageNumber, pageSize, out count),

                //We won't show cateogries list with "Templates" and "AppFrameworks" whose CategoryID are 8 and 9 in database 
                Categories = await _appService.LocalizeCategoriesAsync((await _appService.GetCategoriesAsync()).Where(c => c.CategoryID != 8 && c.CategoryID != 9).ToList(), preferredLanguage),
                SupportedLanguages = await _appService.GetSupportedLanguagesFromFeedAsync(),
                TotalPage = Convert.ToInt32(Math.Ceiling((double)count / pageSize)),
                CurrentSupportedLanguage = supportedLanguage,
                CurrentPage = pageNumber,
                CurrentCategory = (await _appService.LocalizeCategoriesAsync(new List<ProductOrAppCategory> { new ProductOrAppCategory { Name = category } }, preferredLanguage)).FirstOrDefault() ?? new ProductOrAppCategory { Name = category, LocalizedName = category },
                TotalCount = count
            };

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Gallery(string keyword, int? page, string supportedLanguage = Language.CODE_ENGLISH_US)
        {
            var count = 0;
            var pageNumber = page ?? 1;
            var pageSize = 20;
            var preferredLanguage = Thread.CurrentThread.GetLanguageCode();
            var model = new AppGalleryViewModel
            {
                AppList = await _appService.GetAppsFromFeedAsync(keyword, "All", supportedLanguage, preferredLanguage, pageNumber, pageSize, out count),
                SupportedLanguages = await _appService.GetSupportedLanguagesFromFeedAsync(),
                CurrentSupportedLanguage = supportedLanguage,
                TotalPage = Convert.ToInt32(Math.Ceiling(((double)count / pageSize))),
                CurrentPage = pageNumber,
                Keyword = keyword,
                TotalCount = count
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ViewFromFeed(string appId)
        {

            var preferredLanguage = Thread.CurrentThread.GetLanguageCode();
            var submission = await _appService.GetSubmissionFromFeedAsync(appId, preferredLanguage);
            if (submission == null)
            {
                return View("ResourceNotFound");
            }

            var model = new AppDetailViewModel
            {
                Submission = submission,
                Metadata = await _appService.GetMetadataFromFeedAsync(appId, preferredLanguage)
            };

            return View("Preview", model);
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittership]
        public async Task<ActionResult> Preview(int? submissionId)
        {
            if (!submissionId.HasValue)
            {
                return View("ResourceNotFound");
            }

            var submission = await _appService.GetSubmissionAsync(submissionId.Value);
            if (submission == null)
            {
                return View("ResourceNotFound");
            }

            if (!User.IsSuperSubmitter() && !(await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId.Value)))
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            var metadata = await _appService.GetMetadataAsync(submission.SubmissionID);
            if (metadata.Count() == 0)
            {
                return View("NeedAppNameAndDescription", submission.SubmissionID);
            }

            var preferredLanguage = Thread.CurrentThread.GetLanguageCode();
            submission.Categories = await _appService.LocalizeCategoriesAsync(await _appService.GetSubmissionCategoriesAsync(submission.SubmissionID), preferredLanguage);
            var model = new AppDetailViewModel
            {
                IsPreview = true,
                Submission = submission,
                Metadata = await _appService.GetLocalizedMetadataAsync(await _appService.GetMetadataAsync(submissionId.Value), preferredLanguage)
            };

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Install(string appId)
        {
            var preferredLanguage = Thread.CurrentThread.GetLanguageCode();
            var model = new AppInstallViewModel
            {
                Submission = await _appService.GetSubmissionFromFeedAsync(appId, preferredLanguage),
                Metadata = await _appService.GetMetadataFromFeedAsync(appId, preferredLanguage)
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> Delete(int? submissionId, string returnUrl)
        {
            if (!User.IsSuperSubmitter())
            {
                return View("NeedPermission");
            }

            if (submissionId.HasValue)
            {
                await _appService.DeleteAsync(submissionId.Value);
            }

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToRoute(SiteRouteNames.Dashboard);
            else
                return Redirect(returnUrl);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> UpdateStatus(int? submissionId, int? statusId, string returnUrl)
        {
            if (!User.IsSuperSubmitter())
            {
                return View("NeedPermission");
            }

            if (submissionId.HasValue && statusId.HasValue)
            {
                await _appService.UpdateStatusAsync(submissionId.Value, statusId.Value);
            }

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToRoute(SiteRouteNames.Dashboard);
            else
                return Redirect(returnUrl);
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittership]
        public async Task<ActionResult> Publish(int submissionId, string returnUrl)
        {
            if (!User.IsSuperSubmitter())
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            var model = new AppPublishViewModel
            {
                Submission = await _appService.GetPublishingSubmissionAsync(submissionId),
                Packages = await _appService.GetPackagesAsync(submissionId),
                DisplayWarning = !(await _appService.CanBePublishedAsync(submissionId))
            };
            ViewBag.ReturnUrl = returnUrl;

            return View("Publish", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> Publish(int submissionId)
        {
            if (!User.IsSuperSubmitter() && !(await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId)))
            {
                return RedirectToRoute(SiteRouteNames.Portal);
            }

            var submission = await _appService.GetSubmissionAsync(submissionId);
            await _appService.PublishAsync(submission,
                (await _appService.GetMetadataAsync(submissionId)).FirstOrDefault(m => m.Language == Language.CODE_ENGLISH_US),
                await _appService.GetSubmissionCategoriesAsync(submissionId),
                await _appService.GetPackagesAsync(submissionId),
                await _appService.PulishImageUploadAsync(submission, new AppImageAzureStorageService()),
                await _appService.GetDependenciesAsync(submission));

            // the stateID in database of "published" is 7
            await _appService.UpdateStatusAsync(submissionId, 7);

            // send email
            _emailService.SendMessageForSubmissionPublished(User.GetSubmittership(), submission, HttpContext.Request.Url.Authority, html => { return HttpContext.Server.HtmlEncode(html); });

            // once a new app is published, it will appeared at the first page of gallery
            return RedirectToRoute(SiteRouteNames.Gallery);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Mine()
        {
            var submitter = User.GetSubmittership();
            var model = new AppMineViewModel
            {
                HasSubmittership = submitter != null,
                MySubmissions = submitter == null ? null : await _appService.GetMySubmissions(User.GetSubmittership())
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittership]
        public async Task<ActionResult> Owners(int? submissionId)
        {
            if (!submissionId.HasValue) return View("ResourceNotFound");

            var isOwner = await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId.Value);
            if (!User.IsSuperSubmitter() && !isOwner)
            {
                return View("NeedPermission");
            }

            var submission = await _appService.GetSubmissionAsync(submissionId.Value);
            if (submission == null)
            {
                return View("ResourceNotFound");
            }

            var model = new AppOwnersViewModel()
            {
                Submission = submission,
                Owners = await _appService.GetOwnersAsync(submission.SubmissionID),
                OwnershipInvitations = await _appService.GetOwnershipInvitationsAsync(submission.SubmissionID)
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittership]
        public async Task<ActionResult> Verify(int? submissionId, bool? showThanks)
        {
            if (!submissionId.HasValue) return View("ResourceNotFound");

            var isOwner = await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId.Value);
            if (!User.IsSuperSubmitter() && !isOwner)
            {
                return View("NeedPermission");
            }

            var submission = await _appService.GetSubmissionAsync(submissionId.Value);
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
        public async Task<ActionResult> VerifyUrl(string url, string key)
        {
            var status = await _validationService.ValidateUrlAsync(url);

            return Json(new { Status = status.ToString(), Key = key });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPackage(string url, string hash, int submissionId, string key)
        {
            var packageValidation = await _validationService.ValidatePackageAsync(url, hash, submissionId, Server.MapPath("~"));

            return Json(new
            {
                Key = key,
                Result = packageValidation.Result.ToString(),
                PackageValidation = packageValidation,
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyImage(string url, bool isLogo, string key)
        {
            var imageValidationResult = await _validationService.ValidateImageAsync(url, isLogo);

            return Json(new
            {
                TypeStatus = imageValidationResult.TypeStatus.ToString(),
                DimensionStatus = imageValidationResult.DimensionStatus.ToString(),
                Key = key
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> Verify(int submissionId)
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

            return RedirectToRoute(SiteRouteNames.Portal);
        }
    } // end class
}