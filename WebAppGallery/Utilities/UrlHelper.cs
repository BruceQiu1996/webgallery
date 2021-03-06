﻿using System.Net;

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
            catch (WebException e)
            {
                // the exception "The server committed a protocol violation" can also prove that the remote server is exist
                // The HResult of InnerException {"Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host."} is -2146232800
                if (e.Status == WebExceptionStatus.ServerProtocolViolation || (e.InnerException != null && e.InnerException.HResult == -2146232800))
                {
                    statusCode = HttpStatusCode.OK;
                }
            }
            catch { }

            return statusCode == HttpStatusCode.OK;
        }
    }
}