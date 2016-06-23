using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppDetailViewModel
    {
        public Submission Submission { get; set; }
        public SubmissionLocalizedMetaData Metadata { get; set; }

        public string Description
        {
            get
            {
                return string.IsNullOrWhiteSpace(Metadata.Description) ? Metadata.BriefDescription : Metadata.Description;
            }
        }

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
    }
}