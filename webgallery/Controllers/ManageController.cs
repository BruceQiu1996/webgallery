using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using webgallery.Models;
using System.Collections.Generic;
using webgallery.ViewModels;

namespace webgallery.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            /*   ViewBag.StatusMessage =
                   message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                   : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                   : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                   : message == ManageMessageId.Error ? "An error has occurred."
                   : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                   : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                   : "";

               var userId = User.Identity.GetUserId();
               var model = new IndexViewModel
               {
                   HasPassword = HasPassword(),
                   PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                   TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                   Logins = await UserManager.GetLoginsAsync(userId),
                   BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
               };
               return View(model);*/

            ViewBag.Username = User.Identity.GetUserName();
            ViewBag.UserID = User.Identity.GetUserId();
            return View();
        }

        //GET 
        [Authorize]
        public ActionResult Dashboard()
        {
            using (var db = new mscomwebDBEntitiesDB())
            {
               


                IEnumerable<GetAllSubmissionsInBrief_Result> applist = db.GetAllSubmissionsInBrief().ToList<webgallery.Models.GetAllSubmissionsInBrief_Result>();
                return View("Dashboard", applist);
            }

        }
        [Authorize]
        public ActionResult PublisherDetails(int id)
        {
            using (var db = new mscomwebDBEntitiesDB())
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

        public ActionResult Edit(int id)
        {
            using (var db = new mscomwebDBEntitiesDB())
            {
                var submissionToUpdate = from s in db.Submissions
                                         where s.SubmissionID == id
                                         select s;

                var metadata = from m in db.SubmissionLocalizedMetaDatas
                               where m.SubmissionID == id
                               select m;

                var packages = from p in db.Packages
                               where p.SubmissionID == id
                               select p;

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

                var model = new AppSubmissionViewModel {
                    Submission = submissionToUpdate.FirstOrDefault(),
                    Metadata = metadata.ToList(),
                    Packages = packages.ToList(),
                    Languages = Language.SupportedLanguages.ToList(),
                    Categories = categories.ToList(),
                    Frameworks = frameworks.ToList(),
                    DatabaseServers = dbServers.ToList(),
                    WebServerExtensions = webServerExtensions.ToList()
                };

                ChangeDisplayOrder(model.DatabaseServers);

                return View("AppSubmit", model);
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

        public ActionResult Delete(int id)
        {
            using (var db = new mscomwebDBEntitiesDB())
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

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
          

            return RedirectToAction("ManageLogins", new { Message = "Welcome" });
        }

      
       
        // GET: /Manage/ManageLogins
        public ActionResult ManageLogins(ManageMessageId? message)
        {
            /*  ViewBag.StatusMessage =
                  message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                  : message == ManageMessageId.Error ? "An error has occurred."
                  : "";
              var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
              if (user == null)
              {
                  return View("Error");
              }
              var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
              var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
              ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
              return View(new ManageLoginsViewModel
              {
                  CurrentLogins = userLogins,
                  OtherLogins = otherLogins
              });*/

            return View();
        }

        ////
        //// POST: /Manage/LinkLogin
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult LinkLogin(string provider)
        //{
        //    // Request a redirect to the external login provider to link a login for the current user
        //    return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        //}

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}