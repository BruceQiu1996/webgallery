using System;
using System.Web;

namespace WebGallery.Extensions
{
    public static class BrowserExtensions
    {
        public static bool IsInternetExplorer(this HttpBrowserCapabilitiesBase browser)
        {
            return (browser.Browser.ToLowerInvariant().IndexOf("internetexplorer", StringComparison.Ordinal) > -1);
        }
    }
}