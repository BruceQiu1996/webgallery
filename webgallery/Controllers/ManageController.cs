﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Extensions;
using WebGallery.Models;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        public ManageController()
        {
        }

        //
        // GET: /Manage/Index
        public ActionResult Index()
        {
            ViewBag.Name = ClaimsPrincipal.Current.FindFirst("name").Value;
            ViewBag.PreferredUsername = ClaimsPrincipal.Current.FindFirst("preferred_username").Value;
            ViewBag.EmailAddress = ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;

            return View();
        }

        //GET 
        [Authorize]
        public ActionResult Dashboard()
        {
            using (var db = new WebGalleryDbContext())
            {



                //IEnumerable<GetAllSubmissionsInBrief_Result> applist = db.GetAllSubmissionsInBrief().ToList<WebGallery.Models.GetAllSubmissionsInBrief_Result>();
                //return View("Dashboard", applist);

                /*
                 CREATE PROCEDURE [dbo].[GetAllSubmissionsInBrief]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    SELECT 
		Submissions.SubmissionID AS SubmissionID,
		Submissions.Nickname AS Nickname,
		Submissions.Version AS Version,
		Submissions.SubmittingEntity AS SubmittingEntity,
		Submissions.SubmittingEntityURL AS SubmittingEntityURL,
		Submissions.AppURL AS AppURL,
		Submissions.SupportURL AS SupportURL,
		Submissions.ReleaseDate AS ReleaseDate,
		Submissions.FrameworkOrRuntimeID AS FrameworkOrRuntimeID,
		FrameworksAndRuntimes.Name AS FrameworkOrRuntimeName,
		Submissions.DatabaseServerIDs AS DatabaseServerIDs,
		Submissions.WebServerExtensionIDs AS WebServerExtensionIDs,
		Submissions.CategoryID1 AS CategoryID1,
		CategoryName1 = ProductOrAppCategories.Name,
		Submissions.CategoryID2 AS CategoryID2,
		Submissions.LogoID AS LogoID,
		Submissions.ProfessionalServicesURL AS ProfessionalServicesURL,
		Submissions.CommercialProductURL AS CommercialProductURL,
		Submissions.AgreedToTerms AS AgreedToTerms,
		Submissions.AdditionalInfo AS AdditionalInfo,
		SubmissionStateAll.Name AS SubmissionState,
		SubmissionStateAll.SubmissionStateID AS SubmissionStateID,
		SubmissionStateAll.SortOrder AS SubmissionStateSortOrder,
		Submissions.Created AS Created,
		Submissions.Updated AS Updated
    FROM Submissions WITH (NOLOCK)
		INNER JOIN FrameworksAndRuntimes WITH (NOLOCK) ON FrameworksAndRuntimes.FrameworkOrRuntimeID = Submissions.FrameworkOrRuntimeID
		INNER JOIN ProductOrAppCategories WITH (NOLOCK) ON ProductOrAppCategories.CategoryID = Submissions.CategoryID1
		INNER JOIN SubmissionStateAll WITH (NOLOCK) ON SubmissionStateAll.SubmissionID = Submissions.SubmissionID
END
                 */

                return View("Dashboard", null);
            }

        }
        [Authorize]
        public ActionResult PublisherDetails(int id)
        {
            using (var db = new WebGalleryDbContext())
            {

                IEnumerable<SubmissionOwner> owners = db.SubmissionOwners.ToList<SubmissionOwner>();
                var detailspublisher = (from owner in db.SubmissionOwners
                                        join details in db.SubmittersContactDetails on owner.SubmitterID equals details.SubmitterID
                                        where owner.SubmissionID == id
                                        select new
                                        {
                                            SubmissionID = owner.SubmissionID,
                                            OnwerFirstName = details.FirstName,
                                            OnwerLastName = details.LastName,
                                            OnwerEmail = details.EMail,
                                            OnwerAddress1 = details.Address1,
                                            OnwerAddress2 = details.Address2,
                                            OnwerAddress3 = details.Address3,
                                            OnwerCity = details.City,
                                            OnwerCountry = details.Country,
                                            OnwerMiddleName = details.MiddleName,
                                            OnwerState = details.StateOrProvince,
                                            OnwerZipCode = details.ZipOrRegionCode,
                                            OnwerTitle = details.Title,
                                            OnwerPrefix = details.Prefix,
                                            OnwerSuffix = details.Suffix,
                                            OnwerSubmitterID = details.SubmitterID

                                        }).SingleOrDefault();


                PublisherDetails publiserinfo = new PublisherDetails(detailspublisher.SubmissionID, detailspublisher.OnwerSubmitterID, detailspublisher.OnwerTitle, detailspublisher.OnwerPrefix, detailspublisher.OnwerSuffix, detailspublisher.OnwerFirstName, detailspublisher.OnwerLastName, detailspublisher.OnwerMiddleName, detailspublisher.OnwerEmail, detailspublisher.OnwerAddress1, detailspublisher.OnwerAddress2, detailspublisher.OnwerAddress3, detailspublisher.OnwerCity, detailspublisher.OnwerCountry, detailspublisher.OnwerState, detailspublisher.OnwerZipCode);

                return View("PublisherDetails", publiserinfo);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return View("Error");

            AppSubmissionViewModel model = null;
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

                    model = new AppSubmissionViewModel
                    {
                        Submission = submission,
                        MetadataList = metadata.ToList(),
                        Packages = packages.ToList()
                    };
                }
                else
                {
                    model = new AppSubmissionViewModel();
                }

                LoadViewDataForEdit();

                return View("AppSubmit", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, AppSubmissionViewModel model)
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
                    return View("AppSubmit", model);
                }

                // save
                var submission = appService.Submit(model.Submission, model.MetadataList, model.Packages, Request.Files.GetAppImages(), model.GetSettingStatusOfImages(), new AppImageAzureStorageService());

                //
                // send email
                // old site -> AppSubmissionEMailer.SendAppSubmissionMessage(id, ID > 0);

                // go to the App Status page
                // old site -> Response.Redirect("AppStatus.aspx?mode=thanks&id=" + id);
                return RedirectToAction("Status");
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Manage", "Edit"));
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

        static private string _UniqueAppIdValidationLock = "This is used to lock";
        public JsonResult ValidateAppIdVersion(string appId, string version, int? submissionId)
        {
            lock (_UniqueAppIdValidationLock)
            {
                return Json(new AppService().ValidateAppIdVersionIsUnique(appId, version, submissionId));
            }
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