using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppPublishViewModel
    {
        public Submission Submission { get; set; }
        public IList<Package> Packages { get; set; }
        public bool DisplayWarning { get; set; }

    }
}