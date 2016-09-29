using System;
using System.Collections.Generic;
using System.Linq;

namespace WebGallery.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether a specified substring occurs within this string by using a specified StringComparison.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substring">A specified substring</param>
        /// <param name="comp">An equality comparer to compare values</param>
        /// <returns></returns>
        public static bool Contains(this string str, string substring, StringComparison comp)
        {
            if (substring == null) return false;

            if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison", "comp");

            return str.IndexOf(substring, comp) >= 0;
        }

        /// <summary>
        /// Retrieves a substring start at the first character of the string with a spcified length and make it suffix with "..." if the string is longer than the specified length.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length">The specified length</param>
        /// <returns></returns>
        public static string ToShort(this string str, int length)
        {
            return str.Length > length ? str.Substring(0, length) + "..." : str;
        }

        public static bool IsAzureAppId(this string str)
        {
            return AppIdsInAzure.Contains(str, StringComparer.OrdinalIgnoreCase);
        }

        private static List<string> AppIdsInAzure = new List<string>()
        {
            "AcquiaDrupal_MySQL", "AcquiaDrupal_SQL", "AzureMLBESWebAppTemplate", "AzureMLWebAppforRRS",
            "Bakery", "BetterCMS", "BlogEngineNET", "Bottle", "BugNET",
            "CakePHP", "CompositeC1CMS", "Concrete5", "CustomJettySite", "CustomTomcatSite",
            "DigitalXperienceAccelerator", "DjangoFX", "DotNetNuke", "Drupal8", "drupalcommercesql_MySQL", "drupalcommercesql_SQL",
            "eccube", "EmptySite", "Episerver",
            "FlaskV01",
            "galleryserverpro", "Ghost",
            "HTML5Site", "HTMLEmptySite",
            "Incentive",
            "JavaCoffeeShop", "Joomla",
            "Kentico",
            "Lemoon",
            "MageliaWebStore", "Magento", "MediaWiki", "MonoX", "Moodle", "MVCForum", "mycv",
            "NodeJsExpressSite", "NodeJsSite", "NodeJsStarterSite", "nopCommerce", "nService",
            "OpenAtrium", "OpenCart", "OrchardCMS", "osCommerce", "OWA",
            "Personal", "PhotoGallery", "phpBB_MySQL", "phpBB_SQL", "PHPEmptySite", "PiwikAD", "pligg", "ProjectNami",
            "razorCnet",
            "SageFrame", "ServiceGatewayManagementConsole", "simplecms", "SoNETWebEngine", "StarterSite", "SugarCRMad",
            "TYPO3CMS62",
            "Umbraco",
            "vccommunity",
            "WordPress", "WordPressJa"
        };
    }
}