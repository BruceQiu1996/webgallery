using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace webgallery.Models
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
                           Name = c.Name,
                           CultureInfo = c,
                           ShortDisplayName = siblingsExist ? c.DisplayName : c.Parent.DisplayName,
                           TextDirection = c.TextInfo.IsRightToLeft ? Right_To_Left : Left_To_Right
                       };
            }
        }

        private static readonly string[] SupportedLanguageNames = new string[]
        {
            "en-us",
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

        // See http://www.w3schools.com/tags/att_global_dir.asp
        private const string Left_To_Right = "ltr";
        private const string Right_To_Left = "rtl";
    }
}