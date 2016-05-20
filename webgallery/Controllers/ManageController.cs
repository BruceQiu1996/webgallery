using System;
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
            ViewBag.Name = User.GetName();
            ViewBag.PreferredUsername = User.GetPreferredUsername();
            ViewBag.EmailAddress = User.GetEmailAddress();

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