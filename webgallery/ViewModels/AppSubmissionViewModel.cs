using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppSubmissionViewModel
    {
        public Submission Submission { get; set; }
        public ProductOrAppImage Logo { get; set; }
        public IList<SubmissionLocalizedMetaData> MetadataList { get; set; }
        public IList<Package> Packages { get; set; }
    }
}