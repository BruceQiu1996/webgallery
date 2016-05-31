using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Models;
using WebGallery.Security;
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