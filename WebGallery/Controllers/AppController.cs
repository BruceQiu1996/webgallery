using System;
using System.Collections.Generic;
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
            if (!id.HasValue) return View("Error");

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
        public ActionResult Submit(int? id, AppSubmitViewModel model)
        {
            if (!id.HasValue) return View("Error");

            var appService = new AppService();
            try
            {
                // final check
                var finalCheck = appService.ValidateAppIdCharacters(model.Submission.Nickname)
                                && appService.ValidateAppIdVersionIsUnique(model.Submission.Nickname, model.Submission.Version, id);
                if (!finalCheck)
                {
                    LoadViewDataForEdit();
                    ModelState.AddModelError("AppId", "unique");
                    return View(model);
                }

                // save
                var submission = appService.Submit(model.Submission, model.MetadataList, model.Packages, Request.Files.GetAppImages(), model.GetSettingStatusOfImages(), new AppImageAzureStorageService());

                //
                // send email
                // old site -> AppSubmissionEMailer.SendAppSubmissionMessage(id, ID > 0);

                // go to the App Status page
                // old site -> Response.Redirect("AppStatus.aspx?mode=thanks&id=" + id);
                return RedirectToAction("Status", new { id = submission.SubmissionID });
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "App", "Submit"));
            }
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
                return Json(new AppService().ValidateAppIdVersionIsUnique(appId, version, submissionId));
            }
        }

        static private string _UniqueAppIdValidationLock = "This is used to lock";

    } // end class
}