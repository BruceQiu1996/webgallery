using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class AppService : IAppService
    {
        #region validation

        public bool ValidateAppIdVersionIsUnique(string appId, string version, int? submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submission = db.Submissions.FirstOrDefault(
                        s => string.Compare(s.Nickname, appId, StringComparison.InvariantCultureIgnoreCase) == 0
                        && string.Compare(s.Version, version, StringComparison.InvariantCultureIgnoreCase) == 0);

                return (submission != null && submissionId.HasValue)
                    ? submission.SubmissionID == submissionId
                    : submission == null;
            }
        }

        public bool ValidateAppIdCharacters(string appId)
        {
            return Regex.IsMatch(appId, @"^\w*$");
        }

        #endregion

        #region new submission or update an submission

        public Submission Submit(Submitter submitter,
            Submission submission,
            IList<SubmissionLocalizedMetaData> metadataList,
            IList<Package> packages,
            IDictionary<string, AppImage> images,
            IDictionary<string, AppImageSettingStatus> settingStatusOfImages,
            IAppImageStorageService imageStorageService)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submissionInDb = db.Submissions.FirstOrDefault(s => s.SubmissionID == submission.SubmissionID);
                if (submissionInDb == null)
                {
                    throw new ApplicationException($"Can't find the submission #{submission.SubmissionID}");
                }

                // update submission, metadata, packages
                UpdateSubmission(submissionInDb, submission);
                submissionInDb.Updated = DateTime.Now;
                UpdateMetadata(db, metadataList, submission.SubmissionID);
                UpdatePackage(db, packages, submission.SubmissionID);
                db.SaveChanges(); // save the major properties prior to images

                // upload images and update urls
                UploadImages(submissionInDb, images, settingStatusOfImages, imageStorageService);
                db.SaveChanges();

                // if save successfully {
                // set owner of this submissioin if it's a new submission
                //}

                // update submission status
                UpdateSubmissionStatus(db, submitter, submission);
                db.SaveChanges();

                // Add a submisssion transaction
                var description = "Submission State\n";
                description += "    old: New Submission\n";
                description += "    new: Pending Review";
                db.SubmissionTransactions.Add(new SubmissionTransaction
                {
                    SubmissionID = submission.SubmissionID,
                    SubmissionTaskID = 1,
                    Description = description,
                    RecordedAt = DateTime.Now
                });

                db.SaveChanges();

                return submissionInDb;
            }
        }

        private static void UpdateSubmissionStatus(WebGalleryDbContext db, Submitter submitter, Submission submission)
        {
            var submissionStateId = submitter.IsSuperSubmitter()
                                ? 2 // "Testing"
                                : 1 // "Pending Review"
                                ;
            var submisstionStatus = db.SubmissionsStatus.FirstOrDefault(s => s.SubmissionID == submission.SubmissionID);
            if (submisstionStatus == null)
            {
                db.SubmissionsStatus.Add(new SubmissionsStatu
                {
                    SubmissionID = submission.SubmissionID,
                    SubmissionStateID = submissionStateId
                });
            }
            else
            {
                submisstionStatus.SubmissionStateID = submissionStateId;
            }
        }

        private void UpdateSubmission(Submission submissionInDb, Submission submissionFromUser)
        {
            submissionInDb.Nickname = submissionFromUser.Nickname;
            submissionInDb.Version = submissionFromUser.Version;
            submissionInDb.SubmittingEntity = submissionFromUser.SubmittingEntity;
            submissionInDb.SubmittingEntityURL = submissionFromUser.SubmittingEntityURL;
            submissionInDb.AppURL = submissionFromUser.AppURL;
            submissionInDb.SupportURL = submissionFromUser.SupportURL;
            submissionInDb.ReleaseDate = submissionFromUser.ReleaseDate;
            submissionInDb.FrameworkOrRuntimeID = submissionFromUser.FrameworkOrRuntimeID;
            submissionInDb.DatabaseServerIDs = submissionFromUser.DatabaseServerIDs;
            submissionInDb.WebServerExtensionIDs = submissionFromUser.WebServerExtensionIDs;
            submissionInDb.CategoryID1 = submissionFromUser.CategoryID1;
            submissionInDb.CategoryID2 = submissionFromUser.CategoryID2;
            submissionInDb.ProfessionalServicesURL = submissionFromUser.ProfessionalServicesURL;
            submissionInDb.CommercialProductURL = submissionFromUser.CommercialProductURL;
            submissionInDb.AgreedToTerms = submissionFromUser.AgreedToTerms;
            submissionInDb.AdditionalInfo = submissionFromUser.AdditionalInfo;
        }

        private void UpdateMetadata(WebGalleryDbContext db, IList<SubmissionLocalizedMetaData> metadataList, int submissionId)
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

        private void UpdatePackage(WebGalleryDbContext db, IEnumerable<Package> packages, int submissionId)
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

        private void UploadImages(Submission submission, IDictionary<string, AppImage> images, IDictionary<string, AppImageSettingStatus> settingStatusOfImages, IAppImageStorageService imageStorageService)
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

        public bool IsLocked(int submissionId)
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

                return submissionState != null && !stateIdsNotLocked.Contains(submissionState.SubmissionStateID);
            }
        }
    } // class
}