using System.Threading;

namespace WebGallery.Extensions
{
    public static class ThreadExtensions
    {
        public static string GetLanguageCode(this Thread thread)
        {
            return thread.CurrentUICulture.ToString().ToLowerInvariant();
        }
    }
}