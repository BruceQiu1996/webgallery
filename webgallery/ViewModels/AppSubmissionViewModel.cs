﻿using System.Collections.Generic;
using webgallery.Models;

namespace webgallery.ViewModels
{
    public class AppSubmissionViewModel
    {
        public Submission Submission { get; set; }
        public ProductOrAppImage Logo { get; set; }
        public IList<SubmissionLocalizedMetaData> Metadata { get; set; }
        public IList<Package> Packages { get; set; }

        public IList<Language> Languages { get; set; }
        public IList<ProductOrAppCategory> Categories { get; set; }
        public IList<FrameworksAndRuntime> Frameworks { get; set; }
        public IList<DatabaseServer> DatabaseServers { get; set; }
        public IList<WebServerExtension> WebServerExtensions { get; set; }

    }
}