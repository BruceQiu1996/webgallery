using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.Models;
using WebGallery.Utilities;

namespace WebGallery.Services
{
    public class AppValidationService : IAppValidationService
    {
        public Task<List<AppValidationItem>> GetValidationItemsAsync(Submission submission)
        {
            var urlItems1 = new List<AppValidationItem>
            {
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "SubmittingEntityUrl", Value = submission.SubmittingEntityURL},
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "AppWebSiteUrl", Value = submission.AppURL},
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "SupportUrl", Value = submission.SupportURL}
            };
            // Package URL steps will go here (see code below)
            var urlItems2 = new List<AppValidationItem>
            {
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "ProfessionalServicesUrl", Value = submission.ProfessionalServicesURL},
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "CommercialProductUrl", Value = submission.CommercialProductURL},
            };
            // Package manifest and SHA1 hash steps will go here (see code below)
            var imageItems = new List<AppValidationItem>
            {
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "LogoType", Value=submission.LogoUrl},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "LogoDimensions", Value=submission.LogoUrl},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot1Type", Value = submission.ScreenshotUrl1},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot1Dimensions", Value = submission.ScreenshotUrl1},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot2Type", Value = submission.ScreenshotUrl2},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot2Dimensions", Value = submission.ScreenshotUrl2},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot3Type", Value = submission.ScreenshotUrl3},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot3Dimensions", Value = submission.ScreenshotUrl3},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot4Type", Value = submission.ScreenshotUrl4},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot4Dimensions", Value = submission.ScreenshotUrl4},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot5Type", Value = submission.ScreenshotUrl5},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot5Dimensions", Value = submission.ScreenshotUrl5},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot6Type", Value = submission.ScreenshotUrl6},
                new AppValidationItem {Type= AppValidationItemType.Image, Name = "Screenshot6Dimensions", Value = submission.ScreenshotUrl6},
            };

            IEnumerable<Package> packages = null;
            using (var db = new WebGalleryDbContext())
            {
                packages = (from p in db.Packages
                            where p.SubmissionID == submission.SubmissionID
                            select p).ToList();
            }

            // sort on a in-memory collection, that way will speed somehow
            packages = packages.OrderByDescending(p => p.PackageID);

            var packageUrlValidationItems = new List<AppValidationItem>();
            var packageValidationItems = new List<AppValidationItem>();
            foreach (var lang in Language.SupportedLanguages)
            {
                var package = packages.FirstOrDefault(p => p.Language == lang.Name);
                if (package != null)
                {
                    packageUrlValidationItems.Add(new AppValidationItem { Name = "PackageLocationUrl", Type = AppValidationItemType.Url, LanguageAndCountryCode = lang.Name, Value = package.PackageURL });

                    packageValidationItems.Add(new AppValidationItem { Name = "ManifestExists", Type = AppValidationItemType.Package, LanguageAndCountryCode = lang.Name, Value = package.PackageURL });
                    packageValidationItems.Add(new AppValidationItem { Name = "SHA1Hash", Type = AppValidationItemType.Package, LanguageAndCountryCode = lang.Name, Value = package.SHA1Hash });
                }
            }

            return Task.FromResult(
                urlItems1
                .Union(packageUrlValidationItems)
                .Union(urlItems2)
                .Union(packageValidationItems)
                .Union(imageItems)
                .ToList());
        }

        public Task<ValiadationStatus> ValidateUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return Task.FromResult(ValiadationStatus.Unknown);

            return UrlHelper.CanBeAccessed(url)
                ? Task.FromResult(ValiadationStatus.Pass)
                : Task.FromResult(ValiadationStatus.Fail);
        }

        public Task<PackageValidationResult> ValidatePackageAsync(string packageUrl, string hash, int submissionId)
        {
            if (string.IsNullOrWhiteSpace(packageUrl))
            {
                return Task.FromResult(PackageValidationResult.CreateFail());
            }

            using (var stream = StreamHelper.FromUrl(packageUrl))
            {
                // if we cant get a stream from the package url, 
                // then we can't continue with the validation.
                if (stream == null) return Task.FromResult(PackageValidationResult.CreateFail());

                // 1. save FileSize for the package(s) specified by the url
                UpdateFileSizeForPackage(packageUrl, (int)stream.Length, submissionId);

                var result = PackageValidationResult.CreateFail();

                // 2. check if the hashes matches
                result.HashStatus = stream.MatchHash(hash) ? ValiadationStatus.Pass : ValiadationStatus.Fail;

                // 3 .check if the manifest.xml exists
                using (var zipFile = ZipFileHelper.FromStream(stream))
                {
                    result.ManifestStatus = zipFile.ContainsEntry(MANIFEST_FILE_NAME, true) ? ValiadationStatus.Pass : ValiadationStatus.Fail;
                }

                return Task.FromResult(result);
            }
        }

        private static void UpdateFileSizeForPackage(string packageUrl, int fileSize, int submissionId)
        {
            using (var db = new WebGalleryDbContext())
            {
                var packages = (from p in db.Packages
                                where p.SubmissionID == submissionId
                                select p).ToList();

                foreach (var p in packages.Where(p => p.PackageURL == packageUrl))
                {
                    p.FileSize = fileSize;
                }

                db.SaveChanges();
            }
        }

        public Task<ImageValidationResult> ValidateImageAsync(string imageUrl, bool isLogo)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return Task.FromResult(ImageValidationResult.CreateUnknown());
            }

            var maxWidth = isLogo ? LOGO_MAX_WIDTH : SCREENSHOT_MAX_WIDTH;
            var maxHeight = isLogo ? LOGO_MAX_HEIGHT : SCREENSHOT_MAX_HEIGHT;

            using (var img = ImageHelper.FromStream(StreamHelper.FromUrl(imageUrl)))
            {
                return Task.FromResult(new ImageValidationResult
                {
                    TypeStatus = img.IsPng() ? ValiadationStatus.Pass : ValiadationStatus.Fail,
                    DimensionStatus = (img.Width <= maxWidth && img.Height <= maxHeight)
                                    ? ValiadationStatus.Pass
                                    : ValiadationStatus.Fail
                });
            }
        }

        private const string MANIFEST_FILE_NAME = "manifest.xml";
        private const int LOGO_MAX_WIDTH = 200;
        private const int LOGO_MAX_HEIGHT = 200;
        private const int SCREENSHOT_MAX_WIDTH = 800;
        private const int SCREENSHOT_MAX_HEIGHT = 600;
    }
}