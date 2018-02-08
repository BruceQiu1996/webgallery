using PagedList;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class ManageSubmittersViewModel
    {
        public StaticPagedList<SubmittersContactDetail> Submitters { get; set; }
        public string Keyword { get; set; }
        public int PageSize { get; set; }
    }
}