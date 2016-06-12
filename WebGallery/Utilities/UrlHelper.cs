using System.Net;

namespace WebGallery.Utilities
{
    public class UrlHelper
    {
        public static bool CanBeAccessed(string url)
        {
            var statusCode = HttpStatusCode.BadRequest;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0;)"; // IE7 on Vista;
                request.Timeout = 60000; // 1 minute
                request.AllowAutoRedirect = true;
                request.Accept = "*/*";
                request.KeepAlive = true;
                request.CookieContainer = new CookieContainer();
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    statusCode = response.StatusCode;
                    response.Close();
                }
            }
            catch
            {
            }

            return statusCode == HttpStatusCode.OK;
        }
    }
}