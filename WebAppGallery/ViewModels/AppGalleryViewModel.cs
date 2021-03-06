﻿using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppGalleryViewModel
    {
        public IList<Submission> AppList { get; set; }
        public IList<KeyValuePair<string, string>> SupportedLanguages { get; set; }
        public string CurrentSupportedLanguage { get; set; }
        public int CurrentPage;
        public int TotalPage;
        public string Keyword;
        public int TotalCount;
    }
}