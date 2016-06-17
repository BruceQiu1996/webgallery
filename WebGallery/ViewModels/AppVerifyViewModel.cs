using System.Collections.Generic;
using System.Linq;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppVerifyViewModel
    {
        public Submission Submission { get; set; }
        public bool ShowThanks { get; set; }
        public IList<AppValidationItem> ValidationItems { get; set; }
        public IEnumerable<AppValidationItem> Urls { get { return from i in ValidationItems where i.Type == AppValidationItemType.Url select i; } }
        public IEnumerable<AppValidationItem> Packages { get { return from i in ValidationItems where i.Type == AppValidationItemType.Package select i; } }
        public IEnumerable<AppValidationItem> Images { get { return from i in ValidationItems where i.Type == AppValidationItemType.Image select i; } }
    }
}