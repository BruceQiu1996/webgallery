﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class AppService : IAppService
    {
        #region validation

        public Task<bool> ValidateAppIdVersionIsUniqueAsync(string appId, string version, int? submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submission = db.Submissions.FirstOrDefault(
                        s => string.Compare(s.Nickname, appId, StringComparison.InvariantCultureIgnoreCase) == 0
                        && string.Compare(s.Version, version, StringComparison.InvariantCultureIgnoreCase) == 0);
                var isUnique = (submission != null && submissionId.HasValue)
                    ? submission.SubmissionID == submissionId
                    : submission == null;

                return Task.FromResult(isUnique);
            }
        }

        public bool ValidateAppIdCharacters(string appId)
        {
            return Regex.IsMatch(appId, @"^\w*$");
        }

        #endregion

        #region new submission or update an submission

        public Task<Submission> CreateAsync(Submitter submitter,
            Submission submission,
            IList<SubmissionLocalizedMetaData> metadataList,
            IList<Package> packages,
            IDictionary<string, AppImage> images,
            IDictionary<string, AppImageSettingStatus> settingStatusOfImages,
            IAppImageStorageService imageStorageService)
        {
            using (var db = new WebGalleryDbContext())
            {
                submission.Created = DateTime.Now;

                // add submission
                db.Submissions.Add(submission);
                db.SaveChanges();

                // record the relation between the submitter and the submisssion
                db.SubmissionOwners.Add(new SubmissionOwner
                {
                    SubmitterID = submitter.SubmitterID,
                    SubmissionID = submission.SubmissionID
                });
                db.SaveChanges();

                // update metadata/packages with new submission id (> 0)
                foreach (var m in metadataList) m.SubmissionID = submission.SubmissionID;
                foreach (var p in packages) p.SubmissionID = submission.SubmissionID;

                // only complete metadata/pacakges can be saved
                db.SubmissionLocalizedMetaDatas.AddRange(metadataList.Where(m => m.HasCompleteInput()));
                db.Packages.AddRange(packages.Where(p => p.HasCompleteInput()));
                db.SaveChanges();

                // upload images and update urls
                UploadImages(submission, images, settingStatusOfImages, imageStorageService);
                db.SaveChanges();

                // update submission status
                UpdateSubmissionStatus(db, submitter, submission.SubmissionID);
                db.SaveChanges();

                // Add a submisssion transaction
                RecordTransaction(db, submission.SubmissionID);
                db.SaveChanges();

                return Task.FromResult(submission);
            }
        }

        public Task<Submission> UpdateAsync(Submitter submitter,
            Submission submissionInForm,
            IList<SubmissionLocalizedMetaData> metadataList,
            IList<Package> packages,
            IDictionary<string, AppImage> images,
            IDictionary<string, AppImageSettingStatus> settingStatusOfImages,
            IAppImageStorageService imageStorageService)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submissionInDb = db.Submissions.FirstOrDefault(s => s.SubmissionID == submissionInForm.SubmissionID);
                if (submissionInDb == null)
                {
                    throw new ApplicationException($"Can't find the submission #{submissionInForm.SubmissionID}.");
                }

                // update submission
                if (submitter.IsSuperSubmitter())
                {
                    submissionInDb.Nickname = submissionInForm.Nickname; // Only super submitter can change the Nickname(AppId).
                }
                submissionInDb.Updated = DateTime.Now;
                SyncOtherProperties(submissionInDb, submissionInForm);

                // update metadata, packages
                UpdateMetadata(db, metadataList, submissionInForm.SubmissionID);
                UpdatePackage(db, packages, submissionInForm.SubmissionID);
                db.SaveChanges(); // save the major properties prior to images

                // upload images and update urls
                UploadImages(submissionInDb, images, settingStatusOfImages, imageStorageService);
                db.SaveChanges();

                // update submission status
                UpdateSubmissionStatus(db, submitter, submissionInDb.SubmissionID);
                db.SaveChanges();

                // Add a submisssion transaction
                RecordTransaction(db, submissionInDb.SubmissionID);
                db.SaveChanges();

                return Task.FromResult(submissionInDb);
            }
        }

        private static void RecordTransaction(WebGalleryDbContext db, int submissionId)
        {
            var description = "Submission State\n";
            description += "    old: New Submission\n";
            description += "    new: Pending Review";
            db.SubmissionTransactions.Add(new SubmissionTransaction
            {
                SubmissionID = submissionId,
                SubmissionTaskID = 1,
                Description = description,
                RecordedAt = DateTime.Now
            });
        }

        private static void UpdateSubmissionStatus(WebGalleryDbContext db, Submitter submitter, int submissionId)
        {
            var submissionStateId = submitter.IsSuperSubmitter()
                                ? 2 // "Testing"
                                : 1 // "Pending Review"
                                ;
            var submisstionStatus = db.SubmissionsStatus.FirstOrDefault(s => s.SubmissionID == submissionId);
            if (submisstionStatus == null)
            {
                db.SubmissionsStatus.Add(new SubmissionsStatu
                {
                    SubmissionID = submissionId,
                    SubmissionStateID = submissionStateId
                });
            }
            else
            {
                submisstionStatus.SubmissionStateID = submissionStateId;
            }
        }

        private static void SyncOtherProperties(Submission submissionInDb, Submission submissionInFrom)
        {
            submissionInDb.Version = submissionInFrom.Version;
            submissionInDb.SubmittingEntity = submissionInFrom.SubmittingEntity;
            submissionInDb.SubmittingEntityURL = submissionInFrom.SubmittingEntityURL;
            submissionInDb.AppURL = submissionInFrom.AppURL;
            submissionInDb.SupportURL = submissionInFrom.SupportURL;
            submissionInDb.ReleaseDate = submissionInFrom.ReleaseDate;
            submissionInDb.FrameworkOrRuntimeID = submissionInFrom.FrameworkOrRuntimeID;
            submissionInDb.DatabaseServerIDs = submissionInFrom.DatabaseServerIDs;
            submissionInDb.WebServerExtensionIDs = submissionInFrom.WebServerExtensionIDs;
            submissionInDb.CategoryID1 = submissionInFrom.CategoryID1;
            submissionInDb.CategoryID2 = submissionInFrom.CategoryID2;
            submissionInDb.ProfessionalServicesURL = submissionInFrom.ProfessionalServicesURL;
            submissionInDb.CommercialProductURL = submissionInFrom.CommercialProductURL;
            submissionInDb.AgreedToTerms = submissionInFrom.AgreedToTerms;
            submissionInDb.AdditionalInfo = submissionInFrom.AdditionalInfo;
        }

        private static void UpdateMetadata(WebGalleryDbContext db, IList<SubmissionLocalizedMetaData> metadataList, int submissionId)
        {
            var metadataListInDb = db.SubmissionLocalizedMetaDatas.Where(m => m.SubmissionID == submissionId).ToList();

            foreach (var metadata in metadataList)
            {
                var metadataInDb = metadataListInDb.FirstOrDefault(m => m.MetadataID == metadata.MetadataID);
                if (metadata.HasCompleteInput()) // if metadata has complete input
                {
                    if (metadataInDb == null) // if the metadata doesn't exist in database, add a new one
                    {
                        db.SubmissionLocalizedMetaDatas.Add(new SubmissionLocalizedMetaData
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
                        db.SubmissionLocalizedMetaDatas.Remove(metadataInDb);
                    }
                }
            }
        }

        private static void UpdatePackage(WebGalleryDbContext db, IEnumerable<Package> packages, int submissionId)
        {
            var packagesInDb = db.Packages.Where(m => m.SubmissionID == submissionId).ToList();

            foreach (var package in packages)
            {
                var packageInDb = packagesInDb.FirstOrDefault(p => p.PackageID == package.PackageID);
                if (package.HasCompleteInput()) // if package has complete input
                {
                    if (packageInDb == null) // if the package doesn't exist in database, insert a new one
                    {
                        db.Packages.Add(new Package
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
                        db.Packages.Remove(packageInDb);
                    }
                }
            }
        }

        private static void UploadImages(Submission submission, IDictionary<string, AppImage> images, IDictionary<string, AppImageSettingStatus> settingStatusOfImages, IAppImageStorageService imageStorageService)
        {
            if (imageStorageService == null
                || submission == null
                || images == null || images.Count == 0
                || settingStatusOfImages == null || settingStatusOfImages.Count == 0)
                return;

            // logo
            if (images[AppImage.LOGO_IMAGE_NAME].ContentLength > 0)
            {
                submission.LogoUrl = imageStorageService.Upload(submission.SubmissionID, AppImage.LOGO_IMAGE_NAME, images[AppImage.LOGO_IMAGE_NAME].Content);
            }

            // screenshots
            for (var index = 1; index <= 6; index++)
            {
                var image = images[$"{AppImage.SCREENSHOT_IMAGE_NAME_PREFIX}{index}"];
                if (image.ContentLength > 0)
                {
                    var url = imageStorageService.Upload(submission.SubmissionID, image.ImageName, images[image.ImageName].Content);
                    submission.UpdateImageUrl(image.ImageName, url);
                }
                else if (settingStatusOfImages[image.ImageName].WannaDeleteOrReplace) // if the file input has nothing, and the flag of setting is true, then delete the image
                {
                    submission.UpdateImageUrl(image.ImageName, null);
                    imageStorageService.Delete(submission.SubmissionID, image.ImageName);
                }
            }
        }

        #endregion

        public Task<bool> IsModificationLockedAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                int[] stateIdsNotLocked = { 1, // "Pending Review"
                                            3, // "Testing Failed"
                                            5, // "Rejected"
                                            7, // "Published"
                };
                var submissionState = (from state in db.SubmissionStates
                                       join status in db.SubmissionsStatus on state.SubmissionStateID equals status.SubmissionStateID
                                       where status.SubmissionID == submissionId
                                       select state).FirstOrDefault();
                var locked = submissionState != null && !stateIdsNotLocked.Contains(submissionState.SubmissionStateID);

                return Task.FromResult(locked);
            }
        }

        public Task<Submission> GetSubmissionAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submission = (from s in db.Submissions
                                  where s.SubmissionID == submissionId
                                  select s).FirstOrDefault();

                return Task.FromResult(submission);
            }
        }

        public Task<List<SubmissionLocalizedMetaData>> GetMetadataAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var metadata = (from m1 in db.SubmissionLocalizedMetaDatas
                                let ids = from m in db.SubmissionLocalizedMetaDatas
                                          where m.SubmissionID == submissionId
                                          group m by new { m.SubmissionID, m.Language } into g
                                          select g.Max(p => p.MetadataID)
                                where ids.Contains(m1.MetadataID)
                                select m1).ToList();

                return Task.FromResult(metadata);
            }
        }

        public Task<List<Package>> GetPackagesAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var packages = (from p1 in db.Packages
                                let ids = from p in db.Packages
                                          where p.SubmissionID == submissionId
                                          group p by new { p.SubmissionID, p.Language } into g
                                          select g.Max(e => e.PackageID)
                                where ids.Contains(p1.PackageID)
                                select p1).ToList();

                return Task.FromResult(packages);
            }
        }

        public Task<List<Language>> GetSupportedLanguagesAsync()
        {
            return Task.FromResult(Language.SupportedLanguages.ToList());
        }

        public Task<List<ProductOrAppCategory>> GetCategoriesAsync()
        {
            using (var db = new WebGalleryDbContext())
            {
                var categories = (from c in db.ProductOrAppCategories
                                  orderby c.Name
                                  select c).ToList();

                return Task.FromResult(categories);
            }
        }

        public Task<List<FrameworksAndRuntime>> GetFrameworksAsync()
        {
            using (var db = new WebGalleryDbContext())
            {
                var frameworks = (from f in db.FrameworksAndRuntimes
                                  orderby f.Name
                                  select f).ToList();

                return Task.FromResult(frameworks);
            }
        }

        public Task<List<DatabaseServer>> GetDbServersAsync()
        {
            using (var db = new WebGalleryDbContext())
            {
                var dbServers = (from d in db.DatabaseServers
                                 select d).ToList();
                ChangeDisplayOrder(dbServers);

                return Task.FromResult(dbServers);
            }
        }

        private static void ChangeDisplayOrder(IList<DatabaseServer> dbServers)
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

        public Task<List<WebServerExtension>> GetWebServerExtensionsAsync()
        {
            using (var db = new WebGalleryDbContext())
            {
                var extensions = (from e in db.WebServerExtensions
                                  select e).ToList();

                return Task.FromResult(extensions);
            }
        }
    } // class
}