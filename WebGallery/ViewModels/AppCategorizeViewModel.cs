using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppCategorizeViewModel
    {
        public IList<Submission> Submissions { get; set; }
        public IList<ProductOrAppCategory> Categories { get; set; }
        public IList<KeyValuePair<string, string>> SupportedLanguages { get; set; }
        public string CurrentSupportedLanguage { get; set; }
        public ProductOrAppCategory CurrentCategory;
        public int CurrentPage;
        public int TotalPage;
        public int TotalCount;
    }
}