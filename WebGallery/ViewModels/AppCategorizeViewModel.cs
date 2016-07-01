using System.Linq;
using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppCategorizeViewModel
    {
        public IList<Submission> Submissions { get; set; }
        public IList<ProductOrAppCategory> Categories { get; set; }
        public string CurrentCategory;
        public int CurrentPage;
        public int TotalPage;
        public int TotalCount;

        public IList<ProductOrAppCategory> FilteredCategories
        {
            get
            {
                //We won't show these two categories "Templates" and "AppFrameworks" on the page,and their ID in database are 8 and 9
                return Categories.Where(c => c.CategoryID != 8 && c.CategoryID != 9).ToList();
            }
        }
    }
}