using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using WebGallery.Extensions;
using WebGallery.Models;
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
            using (var db = new MsComWebDbContext())
            {
               


                IEnumerable<GetAllSubmissionsInBrief_Result> applist = db.GetAllSubmissionsInBrief().ToList<WebGallery.Models.GetAllSubmissionsInBrief_Result>();
                return View("Dashboard", applist);
            }

        }
        [Authorize]
        public ActionResult PublisherDetails(int id)
        {
            using (var db = new MsComWebDbContext())
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
            using (var db = new MsComWebDbContext())
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
                        Logo = GetImage(submission.LogoID, GetImageQuery(submission.LogoID, db.ProductOrAppImages)),
                        Screenshot1 = GetImage(submission.ScreenshotID1, GetImageQuery(submission.ScreenshotID1, db.ProductOrAppImages)),
                        Screenshot2 = GetImage(submission.ScreenshotID2, GetImageQuery(submission.ScreenshotID2, db.ProductOrAppImages)),
                        Screenshot3 = GetImage(submission.ScreenshotID3, GetImageQuery(submission.ScreenshotID3, db.ProductOrAppImages)),
                        Screenshot4 = GetImage(submission.ScreenshotID4, GetImageQuery(submission.ScreenshotID4, db.ProductOrAppImages)),
                        Screenshot5 = GetImage(submission.ScreenshotID5, GetImageQuery(submission.ScreenshotID5, db.ProductOrAppImages)),
                        Screenshot6 = GetImage(submission.ScreenshotID6, GetImageQuery(submission.ScreenshotID6, db.ProductOrAppImages)),
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

        private IQueryable<ProductOrAppImage> GetImageQuery(int? imageId, IQueryable<ProductOrAppImage> images)
        {
            return from img in images where img.ImageID == imageId select img;
        }

        private ProductOrAppImage GetImage(int? imageId, IQueryable<ProductOrAppImage> query)
        {
            return imageId > 0 ? (query.FirstOrDefault() ?? AppSubmissionViewModel.EmptyImage()) : AppSubmissionViewModel.EmptyImage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, AppSubmissionViewModel model)
        {
            if (!id.HasValue) return View("Error");

            try
            {
                // final check
                var finalCheck = ValidateAppIdCharacters(model.Submission.Nickname) 
                                && ValidateAppIdVersionIsUnique(model.Submission.Nickname, model.Submission.Version, id);
                if (!finalCheck)
                {
                    LoadViewDataForEdit();
                    ModelState.AddModelError("AppId", "unique");
                    return View("AppSubmit", model);
                }

                // save
                SaveAppSubmission(model);

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

        private void SaveAppSubmission(AppSubmissionViewModel model)
        {
            using (var db = new MsComWebDbContext())
            {
                var submission = db.Submissions.FirstOrDefault(s => s.SubmissionID == model.Submission.SubmissionID);
                if (submission == null)
                {
                    View("Error", new Exception(string.Format("Can't found the submission #{0}", model.Submission.SubmissionID)));
                }

                // update submission
                UpdateSubmission(submission, model);
                submission.Updated = DateTime.Now;

                // update metadata
                UpdateMetadata(db.SubmissionLocalizedMetaDatas, model.MetadataList, model.Submission.SubmissionID);

                // update package information
                UpdatePackage(db.Packages, model.Packages, model.Submission.SubmissionID);

                db.SaveChanges();

                // if save successfully {
                // set owner of this submissioin if it's a new submission
                // update values in submission dashboard 
                //}

                // Add a submisssion transaction
                var description = "Submission State\n";
                description += "    old: New Submission\n";
                description += "    new: Pending Review";
                db.SubmissionTransactions.Add(new SubmissionTransaction {
                    SubmissionID = model.Submission.SubmissionID,
                    SubmissionTaskID = 1,
                    Description = description,
                    RecordedAt = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        private void UpdatePackage(DbSet<Package> packageDbSet, IEnumerable<Package> packages, int submissionId)
        {
            var packagesInDb = packageDbSet.Where(m => m.SubmissionID == submissionId).ToList();

            foreach (var package in packages)
            {
                var packageInDb = packagesInDb.FirstOrDefault(p=>p.PackageID == package.PackageID);
                if (package.HasCompleteInput()) // if package has complete input
                {
                    if (packageInDb == null) // if the package doesn't exist in database, insert a new one
                    {
                        packageDbSet.Add(new Package
                        {
                            ArchitectureTypeID = 1, // 1 is for X86
                            PackageURL = package.PackageURL,
                            StartPage = package.StartPage,
                            SHA1Hash = package.SHA1Hash,
                            FileSize = package.FileSize,
                            Language = package.Language,
                            SubmissionID = package.SubmissionID                            
                        });
                    }
                    else // if exists, then update the following 3 fields
                    {
                        packageInDb.PackageURL = package.PackageURL;
                        packageInDb.StartPage = package.StartPage;
                        packageInDb.SHA1Hash = package.SHA1Hash;
                    }
                }
                else // if the package doesn't have complete input (removed by user, or it was empty)
                {
                    if (packageInDb != null) // and if the package exists in database, then remove it
                    {
                        packageDbSet.Remove(packageInDb);
                    }
                }
            }
        }

        private void UpdateMetadata(DbSet<SubmissionLocalizedMetaData> metadataDbSet, IList<SubmissionLocalizedMetaData> metadataList, int submissionId)
        {
            var metadataListInDb = metadataDbSet.Where(m => m.SubmissionID == submissionId).ToList();

            foreach(var metadata in metadataList)
            {
                var metadataInDb = metadataListInDb.FirstOrDefault(m => m.MetadataID == metadata.MetadataID);
                if (metadata.HasCompleteInput()) // if metadata has complete input
                {
                    if (metadataInDb == null) // if the metadata doesn't exist in database, add a new one
                    {
                        metadataDbSet.Add(new SubmissionLocalizedMetaData
                        {
                            SubmissionID = metadata.SubmissionID,
                            Language = metadata.Language,
                            Name = metadata.Name,
                            Description = metadata.Description,
                            BriefDescription = metadata.BriefDescription
                        });
                    }
                    else // if exists, then update the following 3 fields
                    {
                        metadataInDb.Name = metadata.Name;
                        metadataInDb.Description = metadata.Description;
                        metadataInDb.BriefDescription = metadata.BriefDescription;
                    }
                }
                else // if the metadata doesn't have complete input (removed by user, or it was empty)
                {
                    if (metadataInDb != null) // and if the metadata exists in database, then remove it
                    {
                        metadataDbSet.Remove(metadataInDb);
                    }
                }
            }
        }        

        private void UpdateSubmission(Submission submission, AppSubmissionViewModel model)
        {
            submission.Nickname = model.Submission.Nickname;
            submission.Version = model.Submission.Version;
            submission.SubmittingEntity = model.Submission.SubmittingEntity;
            submission.SubmittingEntityURL = model.Submission.SubmittingEntityURL;
            submission.AppURL = model.Submission.AppURL;
            submission.SupportURL = model.Submission.SupportURL;
            submission.ReleaseDate = model.Submission.ReleaseDate;
            submission.FrameworkOrRuntimeID = model.Submission.FrameworkOrRuntimeID;
            submission.DatabaseServerIDs = model.Submission.DatabaseServerIDs;
            submission.WebServerExtensionIDs = model.Submission.WebServerExtensionIDs;
            submission.CategoryID1 = model.Submission.CategoryID1;
            submission.CategoryID2 = model.Submission.CategoryID2;
            submission.ProfessionalServicesURL = model.Submission.ProfessionalServicesURL;
            submission.CommercialProductURL = model.Submission.CommercialProductURL;
            submission.AgreedToTerms = model.Submission.AgreedToTerms;
            submission.AdditionalInfo = model.Submission.AdditionalInfo;
        }

        private void LoadViewDataForEdit()
        {
            using (var db = new MsComWebDbContext())
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
                return Json(ValidateAppIdVersionIsUnique(appId, version, submissionId));
            }
        }

        static private bool ValidateAppIdVersionIsUnique(string appId, string version, int? submissionId)
        {
            using (var db = new MsComWebDbContext())
            {
                var submission = db.Submissions.FirstOrDefault(
                        s => string.Compare(s.Nickname, appId, StringComparison.InvariantCultureIgnoreCase) == 0
                        && string.Compare(s.Version, version, StringComparison.InvariantCultureIgnoreCase) == 0);

                return (submission != null && submissionId.HasValue)
                    ? submission.SubmissionID == submissionId
                    : submission == null;
            }
        }

        static private bool ValidateAppIdCharacters(string appId)
        {
            return Regex.IsMatch(appId, @"^\w*$");
        }

        public ActionResult Delete(int id)
        {
            using (var db = new MsComWebDbContext())
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
    }
}