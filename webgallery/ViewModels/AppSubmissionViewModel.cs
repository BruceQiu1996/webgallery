using System;
using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppSubmissionViewModel
    {
        public Submission Submission { get; set; }
        public ProductOrAppImage Logo { get; set; }
        public ProductOrAppImage Screenshot1 { get; set; }
        public ProductOrAppImage Screenshot2 { get; set; }
        public ProductOrAppImage Screenshot3 { get; set; }
        public ProductOrAppImage Screenshot4 { get; set; }
        public ProductOrAppImage Screenshot5 { get; set; }
        public ProductOrAppImage Screenshot6 { get; set; }
        public IList<SubmissionLocalizedMetaData> MetadataList { get; set; }
        public IList<Package> Packages { get; set; }

        public IList<ProductOrAppImage> Screenshots
        {
            get
            {
                return new List<ProductOrAppImage> {
                    Screenshot1,
                    Screenshot2,
                    Screenshot3,
                    Screenshot4,
                    Screenshot5,
                    Screenshot6 };
            }
        }

        static public ProductOrAppImage EmptyImage()
        {
            return new ProductOrAppImage { ImageID = 0, ImageGUID = Guid.Empty };
        }
    }
}