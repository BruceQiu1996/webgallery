using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using webgallery.Models;

namespace webgallery.ViewModels
{
    public class AppSubmissionViewModel
    {
        public IList<SubmissionLocalizedMetaData> Metadata { get; set; }
        public IList<Package> Packages { get; set; }
        public Submission Submission { get; set; }

        public IList<Language> Languages { get; set; }
        public IList<ProductOrAppCategory> Categories { get; set; }
        public IList<FrameworksAndRuntime> Frameworks { get; set; }
        public IList<DatabaseServer> DatabaseServers { get; set; }
        public IList<WebServerExtension> WebServerExtensions { get; set; }
    }
}