using System.Collections.Generic;
using System.Linq;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppDetailViewModel
    {
        public Submission Submission { get; set; }
        public SubmissionLocalizedMetaData MetaData { get; set; }
        public IList<ProductOrAppCategory> allCategories { get; set; }
        public string AppName
        {
            get
            {
                return Submission.SubmissionID == 0 ? Submission.AppName : MetaData.Name;
            }
        }

        public IList<string> Categories
        {
            get
            {
                if (Submission.SubmissionID != 0)
                {
                    var list = new List<string>();
                    list.Add(allCategories.FirstOrDefault(c => c.CategoryID.ToString() == Submission.CategoryID1).Name);
                    if (!string.IsNullOrWhiteSpace(Submission.CategoryID2) && Submission.CategoryID2 != "0" && Submission.CategoryID1 != Submission.CategoryID2)
                    {
                        list.Add(allCategories.FirstOrDefault(c => c.CategoryID.ToString() == Submission.CategoryID2).Name);
                    }
                    return list;
                }
                else return Submission.Categories;
            }
        }

        public string Description
        {
            get
            {
                return Submission.SubmissionID == 0 ? Submission.BriefDescription : (string.IsNullOrWhiteSpace(MetaData.Description) ? MetaData.BriefDescription : MetaData.Description);
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