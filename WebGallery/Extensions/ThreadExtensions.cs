using System.Threading;

namespace WebGallery.Extensions
{
    public static class ThreadExtensions
    {
        public static string GetLanguage(this Thread thread)
        {
            return thread.CurrentUICulture.ToString().ToLower();
        }
    }
}