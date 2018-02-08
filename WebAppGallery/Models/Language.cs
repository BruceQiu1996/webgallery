using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WebGallery.Models
{
    public class Language
    {
        public string Name { get; set; }
        public CultureInfo CultureInfo { get; set; }
        public string ShortDisplayName { get; set; }
        public string TextDirection { get; set; }

        public static IEnumerable<Language> SupportedLanguages
        {
            get
            {
                var supportedCultures = from lang in SupportedLanguageNames
                                        select new CultureInfo(lang);

                return from c in supportedCultures
                       let siblingsExist = supportedCultures.Any(a => a.Parent.Name == c.Parent.Name && a.Name != c.Name)
                       select new Language
                       {
                           Name = c.Name.ToLowerInvariant(),
                           CultureInfo = c,
                           ShortDisplayName = siblingsExist ? c.DisplayName : c.Parent.DisplayName,
                           TextDirection = c.TextInfo.IsRightToLeft ? Right_To_Left : Left_To_Right
                       };
            }
        }

        private static readonly string[] SupportedLanguageNames = new string[]
        {
            CODE_ENGLISH_US,
            "ar-eg",
            "zh-chs",
            "zh-cht",
            "cs-cz",
            "fr-fr",
            "de-de",
            "he-il",
            "it-it",
            "ja-jp",
            "ko-kr",
            "pl-pl",
            "pt-br",
            "pt-pt",
            "ru-ru",
            "es-es",
            "tr-tr"
        };

        public const string CODE_ENGLISH_US = "en-us";

        // See http://www.w3schools.com/tags/att_global_dir.asp
        private const string Left_To_Right = "ltr";
        private const string Right_To_Left = "rtl";

        public static Dictionary<string, string> ReverseAppLanguageCodeDictionary = new Dictionary<string, string>
        {
            // The codes used to represent languages in the atom feed (XML) are different than what this Web site
            // uses. This dictionary maps from the atom feed code to this Web site's code.
            { "en-us", "en" },
            { "fr-fr", "fr" },
            { "fr-ch", "fr" },
            { "fr-be", "fr" },
            { "nl-be", "en" }, // don't attempt to use fr here
            { "es-es", "es" },
            { "pt-br", "pt-br" },
            { "pt-pt", "pt-pt" },
            { "de-de", "de" },
            { "de-at", "de" },
            { "de-ch", "de" },
            { "it-it", "it" },
            { "ja-jp", "ja" },
            { "ko-kr", "ko" },
            { "ru-ru", "ru" },
            { "cs-cz", "cs" },
            { "pl-pl", "pl" },
            { "zh-chs", "zh-cn" },
            { "zh-cht", "zh-tw" },
            { "tr-tr", "tr" },
            { "he-il", "he" },
            { "ar-eg", "ar" }
       };
    }
}