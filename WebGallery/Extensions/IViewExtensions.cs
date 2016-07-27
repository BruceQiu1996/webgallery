using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace WebGallery.Extensions
{
    public static class IViewExtensions
    {
        private static Dictionary<string, string> fileNameReflection = new Dictionary<string, string>
            {
                {"Me","AccountMe" },
                {"Categorize","Categorize" },
                {"Gallery","Gallery" },
                {"Install","AppInstall" },
                {"Mine","Portal" },
                {"Owners","AppOwners" },
                {"Preview","AppPreview" },
                {"Submit","AppSubmit" },
                {"Verify","AppVerify" },
                {"Detail","InvitationDetail" },
                {"InvitationExpired","InvitationDetail" },
                {"InvitationNotFound","InvitationDetail" },
                {"Send","InvitationSend" }
            };

        private static string GetResourceLanguageCode(string resourceFileName)
        {
            return resourceFileName.IndexOf('.') == resourceFileName.LastIndexOf('.') ? "en-us" : resourceFileName.Substring(resourceFileName.IndexOf('.') + 1, resourceFileName.LastIndexOf('.') - resourceFileName.IndexOf('.') - 1);
        }

        public static List<KeyValuePair<string, string>> GetResourceLanguages(this IView view)
        {
            var viewName = Path.GetFileNameWithoutExtension(((RazorView)view).ViewPath);
            var languages = new List<KeyValuePair<string, string>>();
            string resourceFileName = null;
            if (fileNameReflection.TryGetValue(viewName, out resourceFileName))
            {
                var resourceFiles = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_GlobalResources")).GetFiles("*.resx").Where(f => f.Name.Substring(0, f.Name.IndexOf('.')) == resourceFileName);
                foreach (var r in resourceFiles)
                {
                    var languageCode = GetResourceLanguageCode(r.Name);
                    var nativeName = Regex.Replace(new CultureInfo(languageCode).NativeName, ",[^)]*\\)", ")");
                    languages.Add(new KeyValuePair<string, string>(languageCode, Regex.Replace(nativeName, "\\).*", ")")));
                }
            }

            if (languages.Count() == 0)
            {
                languages.Add(new KeyValuePair<string, string>("en-us", new CultureInfo("en-us").NativeName));
            }

            return languages;
        }
    }
}