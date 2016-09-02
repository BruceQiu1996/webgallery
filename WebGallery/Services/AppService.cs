using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using WebGallery.Extensions;
using WebGallery.Models;

namespace WebGallery.Services
{
    public class AppService : IAppService
    {
        #region validation

        public Task<bool> ValidateAppIdVersionIsUniqueAsync(string appId, string version, int? submissionId)
        {
            lock (_UniqueAppIdValidationLock)
            {
                using (var db = new WebGalleryDbContext())
                {
                    var submission = db.Submissions.FirstOrDefault(
                            s => string.Equals(s.Nickname, appId, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(s.Version, version, StringComparison.OrdinalIgnoreCase));

                    // if not found, then it's unique
                    if (submission == null) return Task.FromResult(true);

                    // if the found submission is itself, then it's unique
                    if (submission.SubmissionID == submissionId) return Task.FromResult(true);

                    // else it's not unique
                    return Task.FromResult(false);
                }
            }
        }

        static private string _UniqueAppIdValidationLock = "This is used to lock";

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
                submission.LogoUrl = imageStorageService.Upload(images[AppImage.LOGO_IMAGE_NAME].Content, submission.Nickname, AppImage.LOGO_IMAGE_NAME, submission.SubmissionID);
            }

            // screenshots
            for (var index = 1; index <= 6; index++)
            {
                var image = images[$"{AppImage.SCREENSHOT_IMAGE_NAME_PREFIX}{index}"];
                if (image.ContentLength > 0)
                {
                    var url = imageStorageService.Upload(images[image.ImageName].Content, submission.Nickname, image.ImageName, submission.SubmissionID);
                    submission.UpdateImageUrl(image.ImageName, url);
                }
                else if (settingStatusOfImages[image.ImageName].WannaDeleteOrReplace) // if the file input has nothing, and the flag of setting is true, then delete the image
                {
                    submission.UpdateImageUrl(image.ImageName, null);
                    imageStorageService.Delete(submission.Nickname, image.ImageName, submission.SubmissionID);
                }
            }
        }

        #endregion

        public Task<bool> IsModificationLockedAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                int[] stateIdsNotLocked = { 1, // "Pending Review"
                                            2, // "Testing"
                                            3, // "Testing Failed"
                                            4, // "Testing Passed"
                                            5, // "Rejected"
                                            6, // "Ready to Publish"
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

        public Task<IList<Submission>> GetMySubmissions(Submitter submitter)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submissions = (from s in db.Submissions
                                   join status in db.SubmissionsStatus on s.SubmissionID equals status.SubmissionID
                                   join state in db.SubmissionStates on status.SubmissionStateID equals state.SubmissionStateID
                                   join u in db.SubmissionOwners on s.SubmissionID equals u.SubmissionID
                                   where u.SubmitterID == submitter.SubmitterID
                                   orderby s.SubmissionID
                                   select new { SubmissionID = s.SubmissionID, Nickname = s.Nickname, Version = s.Version, Status = state.Name }).Distinct().AsEnumerable();

                return Task.FromResult<IList<Submission>>((from s in submissions select new Submission { SubmissionID = s.SubmissionID, Nickname = s.Nickname, Version = s.Version, Status = s.Status }).ToList());
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
            var sqlServerExpress = dbServers.FirstOrDefault(d => string.Equals("SQL Server Express", d.Name, StringComparison.OrdinalIgnoreCase));
            var microsoftSqlDriverForPhp = dbServers.FirstOrDefault(d => string.Equals("Microsoft SQL Driver for PHP", d.Name, StringComparison.OrdinalIgnoreCase));

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

        public Task<IList<SubmittersContactDetail>> GetOwnersAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var owners = (from o in db.SubmissionOwners
                              join c in db.SubmittersContactDetails on o.SubmitterID equals c.SubmitterID
                              where o.SubmissionID == submissionId
                              orderby o.SubmissionOwnerID
                              select c).ToList();

                return Task.FromResult<IList<SubmittersContactDetail>>(owners);
            }
        }

        public Task<IList<UnconfirmedSubmissionOwner>> GetOwnershipInvitationsAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var invitations = (from i in db.UnconfirmedSubmissionOwners
                                   where i.SubmissionID == submissionId
                                   orderby i.UnconfirmedSubmissionOwnerID
                                   select i).ToList();

                return Task.FromResult<IList<UnconfirmedSubmissionOwner>>(invitations);
            }
        }

        public Task MoveToTestingAsync(Submission submission)
        {
            using (var db = new WebGalleryDbContext())
            {
                var testingStateName = "Testing";
                var testingState = (from s in db.SubmissionStates
                                    where s.Name == testingStateName
                                    select s).FirstOrDefault();

                if (testingState == null) throw new TestingStateMissingException();

                var submissionStatus = (from s in db.SubmissionsStatus
                                        where s.SubmissionID == submission.SubmissionID
                                        select s).FirstOrDefault();

                if (submissionStatus == null)
                {
                    db.SubmissionsStatus.Add(new SubmissionsStatu
                    {
                        SubmissionID = submission.SubmissionID,
                        SubmissionStateID = testingState.SubmissionStateID
                    });
                }
                else
                {
                    submissionStatus.SubmissionStateID = testingState.SubmissionStateID;
                }

                db.SaveChanges();

                return Task.FromResult(0);
            }
        }

        /// <summary>
        /// Gets a series of apps from WebApplicationList.xml feed
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="category"></param>
        /// <param name="supportedLanguage"> Language which is supported by packages of the submisions list </param>
        /// <param name="preferredLanguage"> Language in which the page displays </param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public Task<IList<Submission>> GetAppsFromFeedAsync(string keyword, string category, string supportedLanguage, string preferredLanguage, int pageNumber, int pageSize, out int count)
        {
            var xdoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["AppsFeedPath"]));
            var ns = xdoc.Root.GetDefaultNamespace();

            // Another xml feed also should be load according to the preferred Language, first, check whether there exist a feed fit preferred Language
            var resourceElement = xdoc.Root.Element(ns + "resourcesList").Elements(ns + "resources").FirstOrDefault(r => preferredLanguage.Contains(r.Element(ns + "culture").Value) || ("zh-chs".Equals(preferredLanguage) && "zh-cn".Equals(r.Element(ns + "culture").Value)) || ("zh-cht".Equals(preferredLanguage) && "zh-tw".Equals(r.Element(ns + "culture").Value)));

            // If there is not exist a feed fit preferred Language or the culture is "en", Enlish metadata should be used
            bool useEnglishMetaData = resourceElement == null || "en".Equals(resourceElement.Element(ns + "culture").Value);
            var subFeed = useEnglishMetaData ? null : XDocument.Load(resourceElement.Element(ns + "url").Value);

            // The parameter category are the value of xml element keyword, but it's the "id" attribute of xml element keyword who is used in the entry of each app
            var categoryIds = from x in xdoc.Root.Element(ns + "keywords").Elements(ns + "keyword")
                              where category.Equals("all", StringComparison.OrdinalIgnoreCase) || x.Value.Equals(category, StringComparison.OrdinalIgnoreCase)
                              select x.Attribute("id").Value;
            var query = from e in xdoc.Root.Descendants(ns + "entry")
                        let releaseDate = DateTime.Parse(e.Element(ns + "published").Value)

                        // If it use metadata from other language, its title and description is extracted from sub feed, but this element may not be found, then we still use them in English instead
                        let subTitle = useEnglishMetaData ? null : subFeed.Root.Elements("data").FirstOrDefault(d => d.Attribute("name").Value == e.Element(ns + "title").Attribute("resourceName").Value)
                        let title = subTitle == null ? e.Element(ns + "title").Value : subTitle.Value
                        let categories = from c in e.Element(ns + "keywords").Elements(ns + "keywordId")

                                             // in database, categories have "Templates", but it's not exist in feed, if a user published a app whose category is "Templates", its keywords should contain "Templates" and it must can be shown on gallery
                                         where categoryIds.Contains(c.Value) || ((category.Equals("all", StringComparison.OrdinalIgnoreCase) || category.Equals("templates", StringComparison.OrdinalIgnoreCase)) && c.Value.Equals("templates", StringComparison.OrdinalIgnoreCase))
                                         select c.Value
                        let languageIds = from l in e.Element(ns + "installers").Elements(ns + "installer").Elements(ns + "languageId")

                                              // The languageIds in feed are always the substring of the relevant language code ,for example,as a laguageId in feed,the language code of "en" is "en-us".
                                              // So this can be a filter condition , but there exist two special cases : the language code of "zh-cn" is "zh-chs" and the language code of "zh-tw" is "zh-cht"
                                          where supportedLanguage.Contains(l.Value) || ("zh-chs".Equals(supportedLanguage) && "zh-cn".Equals(l.Value)) || ("zh-cht".Equals(supportedLanguage) && "zh-tw".Equals(l.Value))
                                          select l.Value
                        where e.Attribute("type") != null && "application".Equals(e.Attribute("type").Value) && (string.IsNullOrWhiteSpace(keyword) || title.Contains(keyword.Trim(), StringComparison.CurrentCultureIgnoreCase)) && categories.Count() > 0 && languageIds.Count() > 0
                        orderby releaseDate descending
                        select new
                        {
                            nickName = e.Element(ns + "productId").Value,
                            releaseDate = releaseDate,
                            appName = title,
                            version = e.Element(ns + "version").Value,
                            logoUrl = e.Element(ns + "images").Element(ns + "icon") != null ? e.Element(ns + "images").Element(ns + "icon").Value : string.Empty,
                            briefDescription = e.Element(ns + "summary").Value,
                            subBriefDescription = useEnglishMetaData ? null : subFeed.Root.Elements("data").FirstOrDefault(d => d.Attribute("name").Value == e.Element(ns + "summary").Attribute("resourceName").Value)
                        };
            count = query.Count();
            var apps = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsEnumerable();

            return Task.FromResult<IList<Submission>>((from a in apps
                                                       select new Submission
                                                       {
                                                           Nickname = a.nickName,
                                                           ReleaseDate = a.releaseDate,
                                                           AppName = a.appName,
                                                           Version = a.version,
                                                           LogoUrl = a.logoUrl,
                                                           BriefDescription = a.subBriefDescription == null ? a.briefDescription : a.subBriefDescription.Value
                                                       }).ToList());
        }

        public Task<Submission> GetSubmissionFromFeedAsync(string appId, string preferredLanguage)
        {
            var xdoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["AppsFeedPath"]));
            var ns = xdoc.Root.GetDefaultNamespace();

            // Another xml feed also should be load according to the preferred Language, first, check whether there exist a feed fit preferred Language
            var resourceElement = xdoc.Root.Element(ns + "resourcesList").Elements(ns + "resources").FirstOrDefault(r => preferredLanguage.Contains(r.Element(ns + "culture").Value) || ("zh-chs".Equals(preferredLanguage) && "zh-cn".Equals(r.Element(ns + "culture").Value)) || ("zh-cht".Equals(preferredLanguage) && "zh-tw".Equals(r.Element(ns + "culture").Value)));

            // If there is not exist a feed fit preferred Language or the culture is "en", Enlish feed should be used
            bool useEnglish = resourceElement == null || "en".Equals(resourceElement.Element(ns + "culture").Value);
            var subFeed = useEnglish ? null : XDocument.Load(resourceElement.Element(ns + "url").Value);

            var element = (from e in xdoc.Root.Descendants(ns + "entry")
                           where e.Element(ns + "productId").Value.Equals(appId, StringComparison.OrdinalIgnoreCase)
                           select e).FirstOrDefault();
            Submission submission = null;
            if (element != null)
            {
                var keywordElements = from e in element.Element(ns + "keywords").Elements(ns + "keywordId")
                                      join x in xdoc.Root.Element(ns + "keywords").Elements(ns + "keyword") on e.Value equals x.Attribute("id").Value
                                      select x;
                var categories = new List<ProductOrAppCategory>();
                foreach (var k in keywordElements)
                {
                    // If it use feed in other language, its localized category names are extracted from sub feed, but this element may not be found, then we still use them in English instead
                    var LocalizedCateogry = useEnglish ? null : subFeed.Root.Elements("data").FirstOrDefault(l => l.Attribute("name").Value == k.Attribute("resourceName").Value);
                    categories.Add(new ProductOrAppCategory { Name = k.Value, LocalizedName = LocalizedCateogry == null ? k.Value : LocalizedCateogry.Value });
                }

                // special case: the category template exist in database, but there is no such item in keywords element in feed, still, it should also be shown in app preview page
                if (element.Element(ns + "keywords").Elements(ns + "keywordId").Any(k => k.Value.Equals("templates", StringComparison.OrdinalIgnoreCase)))
                {
                    categories.Add(new ProductOrAppCategory { Name = "Templates", LocalizedName = "Templates" });
                }

                var screenshots = new List<string>();
                foreach (var e in element.Element(ns + "images").Elements(ns + "screenshot"))
                {
                    screenshots.Add(e.Value);
                }
                submission = new Submission
                {
                    Nickname = element.Element(ns + "productId").Value,
                    Version = element.Element(ns + "version").Value,
                    SubmittingEntity = element.Element(ns + "author").Element(ns + "name").Value,
                    SubmittingEntityURL = element.Element(ns + "author").Element(ns + "uri").Value,
                    ReleaseDate = DateTime.Parse(element.Element(ns + "published").Value),
                    Categories = categories,
                    LogoUrl = element.Element(ns + "images").Element(ns + "icon") != null ? element.Element(ns + "images").Element(ns + "icon").Value : string.Empty,
                    ScreenshotUrl1 = screenshots.ElementAtOrDefault(0),
                    ScreenshotUrl2 = screenshots.ElementAtOrDefault(1),
                    ScreenshotUrl3 = screenshots.ElementAtOrDefault(2),
                    ScreenshotUrl4 = screenshots.ElementAtOrDefault(3),
                    ScreenshotUrl5 = screenshots.ElementAtOrDefault(4),
                    ScreenshotUrl6 = screenshots.ElementAtOrDefault(5)
                };
            }

            return Task.FromResult(submission);
        }

        public Task<SubmissionLocalizedMetaData> GetMetadataFromFeedAsync(string appId, string preferredLanguage)
        {
            var xdoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["AppsFeedPath"]));
            var ns = xdoc.Root.GetDefaultNamespace();

            // Another xml feed also should be load according to the preferred Language, first, check whether there exist a feed fit preferred Language
            var resourceElement = xdoc.Root.Element(ns + "resourcesList").Elements(ns + "resources").FirstOrDefault(r => preferredLanguage.Contains(r.Element(ns + "culture").Value) || ("zh-chs".Equals(preferredLanguage) && "zh-cn".Equals(r.Element(ns + "culture").Value)) || ("zh-cht".Equals(preferredLanguage) && "zh-tw".Equals(r.Element(ns + "culture").Value)));

            // If there is not exist a feed fit preferred Language or the culture is "en", Enlish feed should be used
            bool useEnglishMetaData = resourceElement == null || "en".Equals(resourceElement.Element(ns + "culture").Value);
            var subFeed = useEnglishMetaData ? null : XDocument.Load(resourceElement.Element(ns + "url").Value);

            var metadata = (from e in xdoc.Root.Descendants(ns + "entry")
                            where e.Element(ns + "productId").Value.Equals(appId, StringComparison.OrdinalIgnoreCase)
                            select new
                            {
                                title = e.Element(ns + "title").Value,
                                subTitle = useEnglishMetaData ? null : subFeed.Root.Elements("data").FirstOrDefault(n => n.Attribute("name").Value == e.Element(ns + "title").Attribute("resourceName").Value),
                                longSummary = e.Element(ns + "longSummary").Value,
                                subLongSummary = useEnglishMetaData ? null : subFeed.Root.Elements("data").FirstOrDefault(n => n.Attribute("name").Value == e.Element(ns + "longSummary").Attribute("resourceName").Value),
                                summary = e.Element(ns + "summary").Value,
                                subSummary = useEnglishMetaData ? null : subFeed.Root.Elements("data").FirstOrDefault(n => n.Attribute("name").Value == e.Element(ns + "summary").Attribute("resourceName").Value)
                            }).FirstOrDefault();

            return Task.FromResult(new SubmissionLocalizedMetaData
            {
                // If it use metadata from other language, its title and descriptions is extracted from sub feed, but this element may not be found, then we still use them in English instead
                Name = metadata.subTitle == null ? metadata.title : metadata.subTitle.Value,
                Description = metadata.subLongSummary == null ? metadata.longSummary : metadata.subLongSummary.Value,
                BriefDescription = metadata.subSummary == null ? metadata.summary : metadata.subSummary.Value
            });
        }

        public Task<IList<ProductOrAppCategory>> GetSubmissionCategoriesAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submission = (from s in db.Submissions
                                  where s.SubmissionID == submissionId
                                  select s).FirstOrDefault();
                var categories = from c in db.ProductOrAppCategories
                                 where c.CategoryID.ToString() == submission.CategoryID1 || c.CategoryID.ToString() == submission.CategoryID2
                                 select c;

                return Task.FromResult<IList<ProductOrAppCategory>>(categories.ToList());
            }
        }

        public Task<IList<ProductOrAppCategory>> LocalizeCategoriesAsync(IList<ProductOrAppCategory> categories, string preferredLanguage)
        {
            //This method is used to localized categories which extracted from database
            var xdoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["AppsFeedPath"]));
            var ns = xdoc.Root.GetDefaultNamespace();

            // Another xml feed also should be load according to the preferred Language, first, check whether there exist a feed fit preferred Language
            var resourceElement = xdoc.Root.Element(ns + "resourcesList").Elements(ns + "resources").FirstOrDefault(r => preferredLanguage.Contains(r.Element(ns + "culture").Value) || ("zh-chs".Equals(preferredLanguage) && "zh-cn".Equals(r.Element(ns + "culture").Value)) || ("zh-cht".Equals(preferredLanguage) && "zh-tw".Equals(r.Element(ns + "culture").Value)));

            // If there is not exist a feed fit preferred Language or the culture is "en", Enlish feed should be used
            bool useEnglish = resourceElement == null || "en".Equals(resourceElement.Element(ns + "culture").Value);
            var subFeed = useEnglish ? null : XDocument.Load(resourceElement.Element(ns + "url").Value);
            var localizedCategories = new List<ProductOrAppCategory>();
            foreach (var c in categories)
            {
                var keyword = xdoc.Root.Element(ns + "keywords").Elements(ns + "keyword").FirstOrDefault(e => e.Value.Equals(c.Name, StringComparison.OrdinalIgnoreCase));
                var localizedName = useEnglish || keyword == null ? null : subFeed.Root.Elements("data").FirstOrDefault(l => l.Attribute("name").Value == keyword.Attribute("resourceName").Value);
                localizedCategories.Add(new ProductOrAppCategory
                {
                    Name = c.Name,
                    LocalizedName = localizedName == null ? c.Name : localizedName.Value
                });
            }

            return Task.FromResult<IList<ProductOrAppCategory>>(localizedCategories);
        }

        public Task<SubmissionLocalizedMetaData> GetLocalizedMetadataAsync(IList<SubmissionLocalizedMetaData> metadatas, string preferredLanguage)
        {
            // This method is used to get the best suited metadata to show when matadatas are extracted from database
            // The most suited metadata is the one whose language is exactly the same with preferred Language
            var metadata = metadatas.FirstOrDefault(m => m.Language.Equals(preferredLanguage, StringComparison.OrdinalIgnoreCase) || ("zh-chs".Equals(m.Language) && "zh-cn".Equals(preferredLanguage)) || ("zh-cht".Equals(m.Language) && "zh-tw".Equals(preferredLanguage)));

            // If there don't exist a metadata whose language are the same with preferred Language completely, we can make a mactching according to their parent culture
            if (metadatas == null)
            {
                metadata = metadatas.FirstOrDefault(m => m.Language.Substring(0, 2).Equals(preferredLanguage.Substring(0, 2)));
            }

            // If we still can't find metadata who has the same parent culture with preferred Language, then we use English
            if (metadata == null)
            {
                metadata = metadatas.FirstOrDefault(m => Language.CODE_ENGLISH_US.Equals(m.Language));
            }

            // If we still can't find metadata in English, we use the first metadata of all
            if (metadatas == null)
            {
                metadata = metadatas.FirstOrDefault();
            }

            return Task.FromResult(metadata);
        }

        public Task<IList<Submission>> GetSubmissionsAsync(string keyword, int page, int pageSize, string sortBy, out int count)
        {
            using (var db = new WebGalleryDbContext())
            {
                keyword = string.IsNullOrWhiteSpace(keyword) ? string.Empty : keyword.Trim();

                var query = from s in db.Submissions
                            join t in db.SubmissionsStatus on s.SubmissionID equals t.SubmissionID
                            join d in db.SubmissionStates on t.SubmissionStateID equals d.SubmissionStateID
                            where keyword == string.Empty || s.Nickname.Contains(keyword)
                            select new
                            {
                                submissionID = s.SubmissionID,
                                nickname = s.Nickname,
                                version = s.Version,
                                created = s.Created,
                                updated = s.Updated,
                                status = d.Name,
                                statusSortOrder = d.SortOrder
                            };

                count = query.Count();

                switch (sortBy)
                {
                    case "appid":
                        query = query.OrderBy(q => q.nickname);
                        break;
                    case "appid_desc":
                        query = query.OrderByDescending(q => q.nickname);
                        break;
                    case "created":
                        query = query.OrderBy(q => q.created);
                        break;
                    case "created_desc":
                        query = query.OrderByDescending(q => q.created);
                        break;
                    case "updated":
                        query = query.OrderBy(q => q.updated);
                        break;
                    case "submissionid":
                        query = query.OrderBy(q => q.submissionID);
                        break;
                    case "submissionid_desc":
                        query = query.OrderByDescending(q => q.submissionID);
                        break;
                    case "status":
                        query = query.OrderBy(q => q.statusSortOrder);
                        break;
                    case "status_desc":
                        query = query.OrderByDescending(q => q.statusSortOrder);
                        break;
                    default:
                        query = query.OrderByDescending(q => q.updated);
                        break;
                }

                var apps = query.Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();

                return Task.FromResult<IList<Submission>>((from a in apps
                                                           select new Submission
                                                           {
                                                               SubmissionID = a.submissionID,
                                                               Nickname = a.nickname,
                                                               Version = a.version,
                                                               Created = a.created,
                                                               Updated = a.updated,
                                                               Status = a.status
                                                           }).ToList());
            }
        }

        public Task<IList<SubmissionState>> GetStatusAsync()
        {
            using (var db = new WebGalleryDbContext())
            {
                var status = (from s in db.SubmissionStates
                              select s).ToList();

                return Task.FromResult<IList<SubmissionState>>(status);
            }
        }

        public Task DeleteAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var status = (from s in db.SubmissionsStatus
                              where s.SubmissionID == submissionId
                              select s).FirstOrDefault();

                //We don not delete the submission record from database, we set its status to "Inactive",
                //the SubmissionStateID of state "Inactive" is 9 in table SubmissionStates
                status.SubmissionStateID = 9;

                //There is only one record in table SubmissionTransactionTypes and its Name is "General"
                var taskId = (from t in db.SubmissionTransactionTypes
                              where t.Name == "General"
                              select t.SubmissionTaskID).FirstOrDefault();

                var transaction = new SubmissionTransaction
                {
                    SubmissionID = submissionId,
                    SubmissionTaskID = taskId,
                    Description = "Delete this submission by setting its status to Inactive",
                    RecordedAt = DateTime.Now
                };

                db.SubmissionTransactions.Add(transaction);
                db.SaveChanges();

                return Task.FromResult(0);
            }
        }

        public Task UpdateStatusAsync(int submissionId, int statusId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var status = (from s in db.SubmissionsStatus
                              where s.SubmissionID == submissionId
                              select s).FirstOrDefault();

                var oldStateName = (from t in db.SubmissionStates
                                    where t.SubmissionStateID == status.SubmissionStateID
                                    select t.Name).FirstOrDefault();

                var newStateName = (from n in db.SubmissionStates
                                    where n.SubmissionStateID == statusId
                                    select n.Name).FirstOrDefault();

                status.SubmissionStateID = statusId;

                var taskId = (from t in db.SubmissionTransactionTypes
                              where t.Name == "General"
                              select t.SubmissionTaskID).FirstOrDefault();

                var transaction = new SubmissionTransaction
                {
                    SubmissionID = submissionId,
                    SubmissionTaskID = taskId,
                    Description = "Submission State\n" + "    old: " + oldStateName + "\n" + "    new: " + newStateName,
                    RecordedAt = DateTime.Now
                };

                db.SubmissionTransactions.Add(transaction);
                db.SaveChanges();

                return Task.FromResult(0);
            }
        }

        private static string Lock_WebApplicationList_Feed = "The lock for WebApplicatonList.xml feed.";

        public Task PublishAsync(Submission submission, SubmissionLocalizedMetaData metadata, IList<ProductOrAppCategory> categories, IList<Package> packages, IList<string> imageUrls)
        {
            lock (Lock_WebApplicationList_Feed)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["AppsFeedPath"]);
                var xdoc = XDocument.Load(path);
                var ns = xdoc.Root.GetDefaultNamespace();
                var oldEntry = (from e in xdoc.Root.Elements(ns + "entry")
                                where e.Element(ns + "productId").Value.Equals(submission.Nickname, StringComparison.OrdinalIgnoreCase) && e.Attribute("type") != null && "application".Equals(e.Attribute("type").Value)
                                select e).FirstOrDefault();

                // create component elements of a new entry
                // create images element
                var imagesElement = new XElement(ns + "images");

                // first, add the icon element to images element
                imagesElement.Add(new XElement(ns + "icon", imageUrls.ElementAtOrDefault(0)));

                // add screenshot elements
                for (int i = 1; i < imageUrls.Count; i++)
                {
                    imagesElement.Add(new XElement(ns + "screenshot", imageUrls.ElementAtOrDefault(i)));
                }

                //create keywords element, it contains categories and database servers
                var keywordsElement = new XElement(ns + "keywords");
                foreach (var c in categories)
                {
                    var keyword = xdoc.Root.Element(ns + "keywords").Elements(ns + "keyword").FirstOrDefault(e => e.Value.Equals(c.Name, StringComparison.OrdinalIgnoreCase));
                    keywordsElement.Add(new XElement(ns + "keywordId", keyword == null ? c.Name : keyword.Attribute("id").Value));
                }

                //add database server keywords
                if (!string.IsNullOrWhiteSpace(submission.DatabaseServerIDs))
                {
                    foreach (var id in submission.DatabaseServerIDs.Split('|'))
                    {
                        // in database the database server IDs of SQL, MySQL and SQL CE are 1, 2 and 4
                        switch (id)
                        {
                            case "1": keywordsElement.Add(new XElement(ns + "keywordId", "SQL")); break;
                            case "2": keywordsElement.Add(new XElement(ns + "keywordId", "MySQL")); break;
                            case "4": keywordsElement.Add(new XElement(ns + "keywordId", "SQLCE")); break;
                            default: break;
                        }
                    }
                }

                //create installers element
                var installersElement = new XElement(ns + "installers");
                for (int i = 0; i < packages.Count(); i++)
                {
                    installersElement.Add(new XElement(ns + "installer",
                        new XElement(ns + "id", (i + 1).ToString()),
                        new XElement(ns + "languageId", Language.ReverseAppLanguageCodeDictionary[packages[i].Language]),
                        new XElement(ns + "osList", new XAttribute("idref", "SupportedAppPlatforms")),
                        new XElement(ns + "installerFile",
                        new XElement(ns + "fileSize", packages[i].FileSize.HasValue ? (packages[i].FileSize.Value / 1024).ToString() : "0"),
                        new XElement(ns + "trackingURL", ConfigurationManager.AppSettings["WebPIHandlerLink"] + "?command=incrementappdownloadcount&appid=" + submission.Nickname + "&version=" + HttpUtility.UrlEncode(submission.Version) + "&applang=" + Language.ReverseAppLanguageCodeDictionary[packages[i].Language]),
                        new XElement(ns + "installerURL", packages[i].PackageURL),
                        new XElement(ns + "sha1", packages[i].SHA1Hash)),
                        new XElement(ns + "msDeploy", new XElement(ns + "startPage", packages[i].StartPage)),
                        new XElement(ns + "helpLink", submission.SupportURL)));
                }

                // create a new entry element, it contains the images element, keywords element, dependency element and installers element which have been created above
                var newEntry = new XElement(ns + "entry", new XAttribute("type", "application"),
                    new XElement(ns + "productId", submission.Nickname),
                    new XElement(ns + "title", metadata.Name, new XAttribute("resourceName", "Entry_" + submission.Nickname + "_Title")),
                    new XElement(ns + "id", ConfigurationManager.AppSettings["WebPI2.0Link"] + submission.Nickname),
                    new XElement(ns + "summary", metadata.BriefDescription, new XAttribute("resourceName", "Entry_" + submission.Nickname + "_Summary")),
                    new XElement(ns + "updated", submission.ReleaseDate.ToUniversalTime().ToString("u").Replace(" ", "T")),
                    new XElement(ns + "published", DateTime.Now.ToUniversalTime().ToString("u").Replace(" ", "T")),
                    new XElement(ns + "longSummary", metadata.Description, new XAttribute("resourceName", "Entry_" + submission.Nickname + "_LongSummary")),
                    new XElement(ns + "version", submission.Version),
                    new XElement(ns + "link", new XAttribute("href", submission.SupportURL)),
                    new XElement(ns + "author",
                    new XElement(ns + "name", submission.SubmittingEntity),
                    new XElement(ns + "uri", submission.SubmittingEntityURL)),
                    imagesElement,
                    keywordsElement,

                    // use the same dependency as original feed, for only the existing applications are updated and new versions of framework won't be added
                    oldEntry.Element(ns + "dependency"),
                    installersElement,

                    // addToFeedDate element is used to record the first time an app added to feed, if there doesn't exist the same app in feed already, its value should be current datetime 
                    new XElement(ns + "addToFeedDate", oldEntry == null ? DateTime.Now.ToUniversalTime().ToString("u").Replace(" ", "T") : oldEntry.Element(ns + "addToFeedDate").Value),
                    new XElement(ns + "pageName", submission.Nickname),
                    new XElement(ns + "productFamily", "Applications", new XAttribute("resourceName", "Applications")));

                // new applications won't be accepted to add to feed, so just replace the old entry
                oldEntry.ReplaceWith(newEntry);
                xdoc.Save(path);

                return Task.FromResult(0);
            }
        }

        public Task<IList<KeyValuePair<string, string>>> GetSupportedLanguagesFromFeedAsync()
        {
            var xdoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["AppsFeedPath"]));
            var ns = xdoc.Root.GetDefaultNamespace();
            var languageIds = from e in xdoc.Root.Elements(ns + "entry").Elements(ns + "installers").Elements(ns + "installer").Elements(ns + "languageId")
                              select e.Value;
            var supportedLanguages = new List<KeyValuePair<string, string>>();
            foreach (var s in Language.SupportedLanguages)
            {
                // If there is no apps in feed support the language ,we won't show it.
                // The languageIds in feed are always the substring of the relevant language code ,for example,as a laguageId in feed,the language code of "en" is "en-us".
                // So this can be a filter condition , but there exist two special cases : the language code of "zh-cn" is "zh-chs" and the language code of "zh-tw" is "zh-cht"
                if (languageIds.Any(i => s.Name.Contains(i) || ("zh-chs".Equals(s.Name) && "zh-cn".Equals(i)) || ("zh-cht".Equals(s.Name) && "zh-tw".Equals(i))))
                {
                    // We don't want to include any long explanations next to the country. For example, in .Net 4, es-es comes out as
                    // Español (España, alfabetización internacional)
                    // But what we really want is something simpler like
                    // Español (España)
                    // So remove everything after the comma and before the closing parenthesis.
                    var nativeName = Regex.Replace(s.CultureInfo.NativeName, ",[^)]*\\)", ")");

                    // Also in .Net 4, after the parentheses, there is sometimes additional information that we consider superfluous to our needs.
                    // For example, the NameName for zh-cht and zh-chs includes "Legacy" after the closing parenthesis. We want to strip that out.
                    nativeName = Regex.Replace(nativeName, "\\).*", ")");
                    supportedLanguages.Add(new KeyValuePair<string, string>(nativeName, s.Name));
                }
            }

            return Task.FromResult<IList<KeyValuePair<string, string>>>(supportedLanguages);
        }

        public Task<Submission> GetPublishingSubmissionAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var submission = (from s in db.Submissions
                                  join t in db.SubmissionsStatus on s.SubmissionID equals t.SubmissionID
                                  join e in db.SubmissionStates on t.SubmissionStateID equals e.SubmissionStateID
                                  join m in db.SubmissionLocalizedMetaDatas on s.SubmissionID equals m.SubmissionID
                                  where s.SubmissionID == submissionId
                                  select new
                                  {
                                      nickName = s.Nickname,
                                      appName = m.Name,
                                      version = s.Version,
                                      created = s.Created,
                                      updated = s.Updated,
                                      status = e.Name,
                                      logoUrl = s.LogoUrl
                                  }).FirstOrDefault();

                return Task.FromResult(new Submission
                {
                    SubmissionID = submissionId,
                    Nickname = submission.nickName,
                    AppName = submission.appName,
                    Version = submission.version,
                    Created = submission.created,
                    Updated = submission.updated,
                    Status = submission.status,
                    LogoUrl = submission.logoUrl
                });
            }
        }

        public Task<bool> CanBePublishedAsync(int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                // for normal user, only submissions whose status is "Testing" or "Testing Passed" can be published, SubmissionStateIDs of "Testing" and "Testing Passed" are 2 and 4
                return Task.FromResult(db.SubmissionsStatus.Any(s => s.SubmissionID == submissionId && (s.SubmissionStateID == 2 || s.SubmissionStateID == 4)));
            }
        }

        public Task<IList<string>> GetDependenciesAsync(Submission submission)
        {
            using (var db = new WebGalleryDbContext())
            {
                var dependencies = new List<string>();

                // get framwork or runtime FeedIDRef of a submission
                var frameworkOrRuntimeFeedIDRef = (from f in db.FrameworksAndRuntimes
                                                   where f.FrameworkOrRuntimeID == submission.FrameworkOrRuntimeID
                                                   select f.FeedIDRef).FirstOrDefault();
                if (frameworkOrRuntimeFeedIDRef != null)
                {
                    dependencies.Add(frameworkOrRuntimeFeedIDRef);
                }

                // get database server FeedIDRefs of a submission
                if (!string.IsNullOrWhiteSpace(submission.DatabaseServerIDs))
                {
                    var databaseServerFeedIDRefs = from id in submission.DatabaseServerIDs.Split('|')
                                                   join d in db.DatabaseServers on id equals d.DatabaseServerID.ToString()
                                                   select d.FeedIDRef;
                    dependencies.AddRange(databaseServerFeedIDRefs);
                }

                // get web server extension FeedIDRefs of a submission
                if (!string.IsNullOrWhiteSpace(submission.WebServerExtensionIDs))
                {
                    var webServerExtensionFeedIDReds = from id in submission.WebServerExtensionIDs.Split('|')
                                                       join w in db.WebServerExtensions on id equals w.WebServerExtensionID.ToString()
                                                       select w.FeedIDRef;
                    dependencies.AddRange(webServerExtensionFeedIDReds);
                }

                return Task.FromResult<IList<string>>(dependencies);
            }
        }

        public Task<IList<string>> PulishImageUploadAsync(Submission submission, IAppImageStorageService imageStorageService)
        {
            // if a new submission is published, we should reupload its images to Azure Storage with a new blob name
            var imageNames = new string[] { AppImage.LOGO_IMAGE_NAME, AppImage.SCREENSHOT_1_IMAGE_NAME, AppImage.SCREENSHOT_2_IMAGE_NAME, AppImage.SCREENSHOT_3_IMAGE_NAME, AppImage.SCREENSHOT_4_IMAGE_NAME, AppImage.SCREENSHOT_5_IMAGE_NAME, AppImage.SCREENSHOT_6_IMAGE_NAME };
            var submissionUrls = new string[] { submission.LogoUrl, submission.ScreenshotUrl1, submission.ScreenshotUrl2, submission.ScreenshotUrl3, submission.ScreenshotUrl4, submission.ScreenshotUrl5, submission.ScreenshotUrl6 };
            var publishUrls = new List<string>();

            // upload images and get the urls
            for (int i = 0; i < submissionUrls.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(submissionUrls[i]))
                {
                    publishUrls.Add(imageStorageService.Upload(new MemoryStream(new WebClient().DownloadData(submissionUrls[i])), submission.Nickname, imageNames[i], null));
                }
            }

            return Task.FromResult<IList<string>>(publishUrls);
        }

        public Task<bool> IsNewAppAsync(string nickName)
        {
            return Task.FromResult(!GetPublishedAppIds().Contains(nickName, StringComparer.OrdinalIgnoreCase));
        }

        private static IList<string> GetPublishedAppIds()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["AppsFeedPath"]);
            var xdoc = XDocument.Load(path);
            var ns = xdoc.Root.GetDefaultNamespace();

            return (from e in xdoc.Root.Elements(ns + "entry")
                    where e.Attribute("type") != null && "application".Equals(e.Attribute("type").Value)
                    select e.Element(ns + "productId").Value).ToList();
        }
    } // class

    public class TestingStateMissingException : Exception
    {
        public TestingStateMissingException() : base("") { }
    }
}