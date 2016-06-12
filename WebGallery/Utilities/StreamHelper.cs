using System;
using System.IO;
using System.Net;

namespace WebGallery.Utilities
{
    public class StreamHelper
    {
        public static MemoryStream FromUrl(string url)
        {
            try
            {
                var start = DateTime.Now;

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Platform-Installer/2.0.0.0(AppGal Automation v1.0)"; // used by the WebPI, bypasses problems like the license popup on CodePlex
                request.Timeout = 90000; // 1.5 minutes
                request.AllowAutoRedirect = true;
                request.Accept = "*/*";
                request.KeepAlive = true;
                request.CookieContainer = new CookieContainer();
                int patience = 90; // seconds until we run out of patience waiting for this validation to complete

                // Even if we get a response within our allotted timeout (1.5 minutes, see above)
                // we still have to actually get all of the bits for the package. That means
                // going back and forth from this Web server to the server where the package
                // is kept, requesting a buffer's worth of bits at a time. If this takes
                // too long, then this Web server will throw an exception, thinking that
                // something has gone wrong with the connection, the response, the app pool,
                // etc. and there is nothing we can do here to catch that. We'll end up
                // going to the generic MS error page when the server thinks an exception
                // has occurred on the server. To avoid that nasty result, we are going to
                // time ourselves here. If we take more than 4 minutes overall we are going to
                // bail out.
                AbortIfDelayed(start, patience);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    AbortIfDelayed(start, patience);
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        AbortIfDelayed(start, patience);

                        var memoryStream = new MemoryStream();
                        var count = 0;
                        var buffer = new byte[4096];
                        do
                        {
                            AbortIfDelayed(start, patience);
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);
                        } while (count != 0);
                        
                        return memoryStream;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static void AbortIfDelayed(DateTime start, int patienceInSeconds)
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > patienceInSeconds)
            {
                throw new ApplicationException("Time out");
            }
        }
    }
}