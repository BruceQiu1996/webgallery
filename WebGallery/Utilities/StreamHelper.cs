using System;
using System.IO;
using System.Net;

namespace WebGallery.Utilities
{
    public class StreamHelper
    {
        public static Stream FromUrl(string url)
        {
            try
            {
                int patience = 240; // seconds until we run out of patience waiting for generating a memory stream/tributary from a response to complete
                var start = DateTime.Now;

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Platform-Installer/2.0.0.0(AppGal Automation v1.0)"; // used by the WebPI, bypasses problems like the license popup on CodePlex
                request.Timeout = 90000; // 1.5 minutes
                request.AllowAutoRedirect = true;
                request.Accept = "*/*";
                request.KeepAlive = true;
                request.CookieContainer = new CookieContainer();

                // Even if we get a response within our allotted timeout (1.5 minutes, see above),
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

                    using (var responseStream = response.GetResponseStream())
                    {
                        AbortIfDelayed(start, patience);

                        var m = new MemoryTributary();
                        var count = 0;
                        var buffer = new byte[4096];
                        do
                        {
                            AbortIfDelayed(start, patience);

                            count = responseStream.Read(buffer, 0, buffer.Length);
                            m.Write(buffer, 0, count);
                        } while (count != 0);

                        return m;
                    }
                }
            }
            catch (AbortGeneratingStreamException e)
            {
                throw e;
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
                throw new AbortGeneratingStreamException(patienceInSeconds);
            }
        }

        public static FileInfo DownloadFrom(string url, string filePath)
        {
            try
            {
                int patience = 240; // seconds until we run out of patience waiting for generating a memory stream/tributary from a response to complete
                var start = DateTime.Now;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko/20100101 Firefox/12.0";
                request.Timeout = 90000; // 1.5 minutes
                request.AllowAutoRedirect = true;
                request.Accept = "*/*";
                request.KeepAlive = true;
                request.CookieContainer = new CookieContainer();

                // Even if we get a response within our allotted timeout (1.5 minutes, see above),
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

                    using (var responseStream = response.GetResponseStream())
                    {
                        AbortIfDelayed(start, patience);
                        using (var fs = File.Create(filePath))
                        {
                            var count = 0;
                            var buffer = new byte[4096];
                            do
                            {
                                AbortIfDelayed(start, patience);

                                count = responseStream.Read(buffer, 0, buffer.Length);
                                fs.Write(buffer, 0, count);
                            } while (count != 0);
                        }
                    }
                }

                return new FileInfo(filePath);
            }
            catch (AbortGeneratingStreamException e)
            {
                throw e;
            }
            catch
            {
                return null;
            }
        }
    }

    public class AbortGeneratingStreamException : Exception
    {
        public AbortGeneratingStreamException(int patienceInSeconds)
            : base($"Generating a stream aborted as it took more time than we expect ({patienceInSeconds}).")
        { }
    }
}