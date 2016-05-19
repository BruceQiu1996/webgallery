using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppSubmitViewModel
    {
        public Submission Submission { get; set; }
        public IList<SubmissionLocalizedMetaData> MetadataList { get; set; }
        public IList<Package> Packages { get; set; }

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
    }
}