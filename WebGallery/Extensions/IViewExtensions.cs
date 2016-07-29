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
        // language codes of each page determined by the amount of resource files, in the old site, resource string of each page are from "Gallery.resx" and "Submit.resx", their supported languages are different
        // some pages are only have resource files in two languages 
        private static string[] codesFromSubmit = { "es-es", "ja-jp", "ko-kr", "en-us", "tr-TR", "zh-chs", "zh-cht" };
        private static string[] codesInTwoLanguages = { "en-us", "tr-TR" };
        private static string[] codesFromGalllery = { "de-at", "de-ch", "de-de", "el-gr", "es-es", "fr-ch", "fr-fr", "hu-hu", "it-it", "ja-jp", "ko-kr", "pl-pl", "en-us", "ru-ru", "tr-TR", "zh-chs", "zh-cht" };
        private static Dictionary<string, string[]> ViewCodesReflection = new Dictionary<string, string[]>
            {
                {"Me",codesFromSubmit },
                {"Submit",codesFromSubmit },
                {"Verify",codesFromSubmit },
                {"Detail",codesFromSubmit },
                {"InvitationExpired",codesFromSubmit },
                {"InvitationNotFound",codesFromSubmit },
                {"Send",codesFromSubmit },
                {"Categorize",codesFromGalllery },
                {"Gallery",codesFromGalllery },
                {"Install",codesFromGalllery },
                {"Preview",codesFromGalllery },
                {"Mine",codesInTwoLanguages },
                {"Owners",codesInTwoLanguages }
            };

        public static List<KeyValuePair<string, string>> GetLanguageCodes(this IView view)
        {
            var viewName = Path.GetFileNameWithoutExtension(((RazorView)view).ViewPath);
            var languages = new List<KeyValuePair<string, string>>();
            string[] codes = null;
            if (ViewCodesReflection.TryGetValue(viewName, out codes))
            {
                foreach (var c in codes)
                {
                    var nativeName = Regex.Replace(new CultureInfo(c).NativeName, ",[^)]*\\)", ")");
                    languages.Add(new KeyValuePair<string, string>(c, Regex.Replace(nativeName, "\\).*", ")")));
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