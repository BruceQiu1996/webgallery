using System;
using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppSubmitViewModel
    {
        public Submission Submission { get; set; }
        public IList<SubmissionLocalizedMetaData> MetadataList { get; set; }
        public IList<Package> Packages { get; set; }

        public bool CanEditNickname { get; set; } = true;

        public bool IsNewSubmission { get; set; } = false;

        public IList<string> ScreenshotUrls
        {
            get
            {
                return new List<string>
                {
                    Submission.ScreenshotUrl1,
                    Submission.ScreenshotUrl2,
                    Submission.ScreenshotUrl3,
                    Submission.ScreenshotUrl4,
                    Submission.ScreenshotUrl5,
                    Submission.ScreenshotUrl6
                };
            }
        }

        public IList<bool> SettingStatusOfScreenshots
        {
            get
            {
                return new List<bool>
                {
                    SetScreenshot1,
                    SetScreenshot2,
                    SetScreenshot2,
                    SetScreenshot4,
                    SetScreenshot5,
                    SetScreenshot6
                };
            }
        }

        public bool SetLogo { get; set; } = false;
        public bool SetScreenshot1 { get; set; } = false;
        public bool SetScreenshot2 { get; set; } = false;
        public bool SetScreenshot3 { get; set; } = false;
        public bool SetScreenshot4 { get; set; } = false;
        public bool SetScreenshot5 { get; set; } = false;
        public bool SetScreenshot6 { get; set; } = false;

        internal static AppSubmitViewModel Empty()
        {
            return new AppSubmitViewModel
            {
                Submission = new Submission
                {
                    ReleaseDate = DateTime.Now,
                },
                MetadataList = new List<SubmissionLocalizedMetaData>(),
                Packages = new List<Package>(),
                IsNewSubmission = true
            };
        }

        internal static AppSubmitViewModel Fake()
        {
            var submission = new Submission()
            {
                SubmissionID = 0,
                Nickname = "SampleApp",
                Version = "1.0.0",
                SubmittingEntity = "Groovy bits",
                SubmittingEntityURL = "http://www.groovybits.com",
                AppURL = "http://www.groovybits.com/SampleApp/about",
                SupportURL = "http://www.groovybits.com/SampleApp/support",
                ReleaseDate = new DateTime(2009, 2, 7),

                FrameworkOrRuntimeID = 1,
                DatabaseServerIDs = string.Empty,
                WebServerExtensionIDs = string.Empty,

                CategoryID1 = "1",
                CategoryID2 = "4",

                ProfessionalServicesURL = "http://www.groovybits.com/SampleApp/ProfessionalServices",
                CommercialProductURL = "http://www.groovybits.com/SampleApp/EnterpriseUpgrade",
                AgreedToTerms = true,
                AdditionalInfo = "Please expedite the processing of this submission. THANKS.",
            };

            var metadatum = new SubmissionLocalizedMetaData
            {
                MetadataID = 0,
                SubmissionID = submission.SubmissionID,
                Language = Language.CODE_ENGLISH_US,
                Name = "SampleApp",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi eget euismod odio. Donec consectetur aliquam metus, sed sagittis ante ultricies id. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed risus lorem, mollis non facilisis ullamcorper, consectetur quis ante. Sed dignissim est sed ipsum accumsan imperdiet. Nam faucibus scelerisque arcu, sed pellentesque orci rutrum at. Aliquam quis accumsan ipsum. Etiam suscipit sapien quis ipsum venenatis convallis. Duis vel ligula elit, ac sagittis ante. Integer non sem dolor, in congue erat. Pellentesque pharetra magna a nunc viverra blandit. Sed tincidunt suscipit nisi, quis placerat felis ultricies in. Sed elementum consectetur arcu vel imperdiet. Cras suscipit tempus scelerisque. Donec turpis felis, mollis non sodales sed, suscipit vel massa. Pellentesque eget mauris ac lorem molestie aliquet. Nunc tempor nulla sed lacus iaculis at molestie urna hendrerit. Mauris iaculis massa sed mauris feugiat eget vulputate est varius. Maecenas in dictum orci.",
                BriefDescription = "This is a fake WebPI application to test the Automation process for submission of Web apps."
            };

            var package = new Package
            {
                PackageID = 0,
                SubmissionID = submission.SubmissionID,
                Language = Language.CODE_ENGLISH_US,
                ArchitectureTypeID = 1,
                FileSize = 0,
                MD5Hash = null,
                PackageURL = "http://www.groovybits.com/SampleApp/SampleApp.zip",
                StartPage = "/X86SampleAppStart.htm",
                SHA1Hash = "5A16A37AF467FB4CDE961759DEC2AD167C689BC5"
            };

            return new AppSubmitViewModel()
            {
                Submission = submission,
                MetadataList = new List<SubmissionLocalizedMetaData> { metadatum },
                Packages = new List<Package> { package }
            };
        }

        internal static AppSubmitViewModel Clone(Submission submission, IList<SubmissionLocalizedMetaData> metadata, IList<Package> packages)
        {
            submission.SubmissionID = 0;
            submission.LogoUrl =
                submission.ScreenshotUrl1 =
                submission.ScreenshotUrl2 =
                submission.ScreenshotUrl3 =
                submission.ScreenshotUrl4 =
                submission.ScreenshotUrl5 =
                submission.ScreenshotUrl6 = null;

            foreach (var m in metadata) m.SubmissionID = m.MetadataID = 0;
            foreach (var p in packages) p.SubmissionID = p.PackageID = 0;

            return new AppSubmitViewModel { Submission = submission, MetadataList = metadata, Packages = packages };
        }
    }
}