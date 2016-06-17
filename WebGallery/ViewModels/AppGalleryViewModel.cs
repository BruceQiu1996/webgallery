using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppGalleryViewModel
    {
        public IQueryable<AppAbstract> AppList { get; set; }
        public int CurrentPage;
        public int TotalPage;
        public string Keyword;
    }
}