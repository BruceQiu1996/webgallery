using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppMineViewModel
    {
        public bool HasSubmittership { get; set; }
        public IList<Submission> MySubmissions { get; set; }
    }
}