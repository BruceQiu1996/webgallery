using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Extensions;
using WebGallery.Models;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class AppController : Controller
    {
        private IAppService _appService;
        private ISubmitterService _submitterService;

        public AppController(IAppService appService, ISubmitterService submitterService)
        {
            _appService = appService ?? new AppService();
            _submitterService = submitterService ?? new SubmitterService();
        }

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
        public async Task<ActionResult> Submit(int? id)
        {
            // Check if the browser is IE and its version is less than 10.
            // If yes, then the user will be prompted to upgrade IE because the page uses placeholder (a HTML 5 attribute) to show watermark text.
            // See http://www.w3schools.com/tags/att_input_placeholder.asp.
            var browser = Request.Browser.Browser.ToLowerInvariant();
            if (browser.IndexOf("ie", StringComparison.Ordinal) > -1 && Request.Browser.MajorVersion < 10)
            {
                return View("UpgradeIE", HttpContext.GetGlobalResourceObject("Submit", "UpgradeIE"));
            }

            // Check if submitting app is enabled
            if (ConfigurationManager.AppSettings["EnableSubmitApp"].ToLower() == "false")
            {
                return View("Error");
            }

            var submitter = _submitterService.GetSubmitterByMicrosoftAccount(User.GetMicrosoftAccount());
            if (submitter == null)
            {
                return View("Error");
            }

            // Check if current submitter who is not Super Submitter has contact info.
            if (!submitter.IsSuperSubmitter() && _submitterService.HasContactInfo(submitter.SubmitterID))
            {
                return RedirectToAction("Profile", "Account");
            }

            int submissionId = id.Value;

            // Check if current user can modify the app.
            // Only the owner and a super submitter can do that.
            if (!submitter.IsSuperSubmitter() && !_submitterService.IsOwner(submitter.SubmitterID, submissionId))
            {
                return View("Error");
            }

            // Check if the app is locked.
            if (_appService.IsLocked(submissionId))
            {
                // disable the form and display "This submission is being reviewed and processed by Microsoft Corp. No modifications can be made at this time."
                return View("Error");
            }

            AppSubmitViewModel model = null;
            using (var db = new WebGalleryDbContext())
            {
                var submission = (from s in db.Submissions
                                  where s.SubmissionID == id
                                  select s).FirstOrDefault();

                if (submission != null)
                {
                    var metadata = from m1 in db.SubmissionLocalizedMetaDatas
                                   let ids = from m in db.SubmissionLocalizedMetaDatas
                                             where m.SubmissionID == id
                                             group m by new { m.SubmissionID, m.Language } into g
                                             select g.Max(p => p.MetadataID)
                                   where ids.Contains(m1.MetadataID)
                                   select m1;
                    var packages = from p1 in db.Packages
                                   let ids = from p in db.Packages
                                             where p.SubmissionID == id
                                             group p by new { p.SubmissionID, p.Language } into g
                                             select g.Max(e => e.PackageID)
                                   where ids.Contains(p1.PackageID)
                                   select p1;

                    model = new AppSubmitViewModel
                    {
                        Submission = submission,
                        MetadataList = metadata.ToList(),
                        Packages = packages.ToList()
                    };
                }
                else
                {
                    model = new AppSubmitViewModel();
                }

                LoadViewDataForEdit();

                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(AppSubmitViewModel model)
        {
            var submitter = _submitterService.GetSubmitterByMicrosoftAccount(User.GetMicrosoftAccount());
            if (submitter == null)
            {
                return View("Error");
            }

            // Check if current user can modify the app.
            if (!submitter.IsSuperSubmitter() && !_submitterService.IsOwner(submitter.SubmitterID, model.Submission.SubmissionID))
            {
                return View("Error");
            }

            // final check
            var finalCheck = _appService.ValidateAppIdCharacters(model.Submission.Nickname)
                                && _appService.ValidateAppIdVersionIsUnique(model.Submission.Nickname, model.Submission.Version, model.Submission.SubmissionID);
            if (!finalCheck)
            {
                LoadViewDataForEdit();
                ModelState.AddModelError("AppId", "unique");
                return View(model);
            }

            // save
            var submission = _appService.Submit(submitter, model.Submission, model.MetadataList, model.Packages, Request.Files.GetAppImages(), model.GetSettingStatusOfImages(), new AppImageAzureStorageService());

            //
            // send email
            // old site -> AppSubmissionEMailer.SendAppSubmissionMessage(id, ID > 0);

            // go to the App Status page
            // old site -> Response.Redirect("AppStatus.aspx?mode=thanks&id=" + id);
            return RedirectToAction("Status", new { id = submission.SubmissionID });
        }

        private void LoadViewDataForEdit()
        {
            using (var db = new WebGalleryDbContext())
            {
                var categories = from c in db.ProductOrAppCategories
                                 orderby c.Name
                                 select c;

                var frameworks = from f in db.FrameworksAndRuntimes
                                 orderby f.Name
                                 select f;
                var dbServers = from d in db.DatabaseServers
                                select d;

                var webServerExtensions = from e in db.WebServerExtensions
                                          select e;

                ViewBag.Languages = Language.SupportedLanguages.ToList();
                ViewBag.Categories = categories.ToList();
                ViewBag.Frameworks = frameworks.ToList();
                ViewBag.DatabaseServers = dbServers.ToList();
                ViewBag.WebServerExtensions = webServerExtensions.ToList();

                ChangeDisplayOrder(ViewBag.DatabaseServers);
            }
        }

        private void ChangeDisplayOrder(IList<DatabaseServer> dbServers)
        {
            // We always want "Microsoft SQL Driver for PHP" immediately after SQL Server Express because the 2 are related.
            // See the line #1074 in the old AppSubmit.aspx.cs            
            var sqlServerExpress = dbServers.FirstOrDefault(d => string.Compare("SQL Server Express", d.Name, StringComparison.OrdinalIgnoreCase) == 0);
            var microsoftSqlDriverForPhp = dbServers.FirstOrDefault(d => string.Compare("Microsoft SQL Driver for PHP", d.Name, StringComparison.OrdinalIgnoreCase) == 0);

            if (sqlServerExpress != null && microsoftSqlDriverForPhp != null)
            {
                dbServers.Remove(microsoftSqlDriverForPhp);
                dbServers.Insert(dbServers.IndexOf(sqlServerExpress) + 1, microsoftSqlDriverForPhp);
            }
        }


        public JsonResult ValidateAppIdVersion(string appId, string version, int? submissionId)
        {
            lock (_UniqueAppIdValidationLock)
            {
                return Json(_appService.ValidateAppIdVersionIsUnique(appId, version, submissionId));
            }
        }

        static private string _UniqueAppIdValidationLock = "This is used to lock";

    } // end class
}