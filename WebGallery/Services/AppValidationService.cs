using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.Models;
using WebGallery.Services.SIR;
using WebGallery.Utilities;

namespace WebGallery.Services
{
    public class AppValidationService : IAppValidationService
    {
        public Task<List<AppValidationItem>> GetValidationItemsAsync(Submission submission)
        {
            var urlItems = new List<AppValidationItem>
            {
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "SubmittingEntityUrl", Value = submission.SubmittingEntityURL},
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "AppWebSiteUrl", Value = submission.AppURL},
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "SupportUrl", Value = submission.SupportURL},
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "ProfessionalServicesUrl", Value = submission.ProfessionalServicesURL},
                new AppValidationItem {Type = AppValidationItemType.Url, Name = "CommercialProductUrl", Value = submission.CommercialProductURL},
            };

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

            var packageValidationItems = new List<AppValidationItem>();
            foreach (var lang in Language.SupportedLanguages)
            {
                var package = packages.FirstOrDefault(p => p.Language == lang.Name);
                if (package != null)
                {
                    packageValidationItems.Add(new AppValidationItem { Name = package.PackageURL, Type = AppValidationItemType.Package, LanguageAndCountryCode = lang.Name, Value = package.SHA1Hash });
                }
            }

            return Task.FromResult(
                urlItems
                .Union(imageItems)
                .Union(packageValidationItems)
                .ToList());
        }

        public Task<ValidationResult> ValidateUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return Task.FromResult(ValidationResult.Unknown);

            return UrlHelper.CanBeAccessed(url)
                ? Task.FromResult(ValidationResult.Pass)
                : Task.FromResult(ValidationResult.Fail);
        }

        public Task<PackageValidation> ValidatePackageAsync(string packageUrl, string sha1HashToValidate, int submissionId, string workingFolder)
        {
            lock (Lock_Verify_Package)
            {
                if (string.IsNullOrWhiteSpace(packageUrl))
                {
                    return Task.FromResult(PackageValidation.Fail(packageUrl, sha1HashToValidate, workingFolder, "The package URL is empty or contains white space(s)."));
                }

                var packageFileName = Guid.NewGuid().ToString() + "package.zip";
                var tempPackagePath = Path.Combine(workingFolder, $@"SIR\temp\{packageFileName}");

                // download this package and save it under ~\SIR\temp\
                var packageZipFile = StreamHelper.DownloadFrom(packageUrl, tempPackagePath);
                if (packageZipFile == null)
                {
                    return Task.FromResult(PackageValidation.Fail(packageUrl, sha1HashToValidate, workingFolder, "This package can't be downloaded for validating."));
                }

                // update FileSize for the package specified by the url
                UpdateFileSizeForPackage(packageUrl, (int)packageZipFile.Length, submissionId);

                // start SIR validation
                var packageValidation = new PackageValidation(tempPackagePath, sha1HashToValidate, workingFolder);
                var validator = new SirPackageValidator(packageValidation);
                try
                {
                    validator.Validate();
                }
                catch (Exception ex)
                {
                    packageValidation.ErrorMessage = ex.Message;
                    packageValidation.Result = AppGallery.SIR.ILog.ValidationResult.Fail;
                }

                // try to delete the package zip file
                try { packageZipFile.Delete(); }
                catch { } // it's okay if deleting throws exceptions.

                return Task.FromResult(packageValidation);
            }
        }
        private static string Lock_Verify_Package = "The lock for package verification.";

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
                    TypeStatus = img.IsPng() ? ValidationResult.Pass : ValidationResult.Fail,
                    DimensionStatus = (img.Width <= maxWidth && img.Height <= maxHeight)
                                    ? ValidationResult.Pass
                                    : ValidationResult.Fail
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