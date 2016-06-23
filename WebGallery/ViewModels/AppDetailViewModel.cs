using System.Collections.Generic;
using System.Linq;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppDetailViewModel
    {
        public Submission Submission { get; set; }
        public SubmissionLocalizedMetaData MetaData { get; set; }

        public string Description
        {
            get
            {
                return string.IsNullOrWhiteSpace(MetaData.Description) ? MetaData.BriefDescription : MetaData.Description;
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