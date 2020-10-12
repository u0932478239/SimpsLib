using System;
using System.IO;
using System.Net;
using SimpsLib.HTTP;

namespace SimpsLib
{
    public class Web
    {
        public static void downloadFileSilent(string url, string path = null, string useragent = null)
        {
            WebClient wc = new WebClient();
            Uri address = new Uri(url);
            wc.Proxy = null;
            if (useragent != null)
            {
                wc.Headers.Add(useragent);
            }
            if (path == null)
            {
                wc.DownloadFileAsync(address, Path.GetFileName(address.LocalPath));
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                wc.DownloadFileAsync(address, path);
            }
        }

        public static string downloadStringSilent(string url, string useragent = null)
        {
            WebClient wc = new WebClient();
            wc.Proxy = null;
            Uri address = new Uri(url);
            if (useragent != null)
            {
                wc.Headers.Add(useragent);
            }
            return wc.DownloadString(address);
        }

        public static string getRequest(string url, string useragent = null, string authorization = null)
        {
            HttpRequest wr = new HttpRequest();
            wr.Proxy = null;
            wr.KeepAlive = true;
            wr.IgnoreProtocolErrors = true;

            if (useragent != null)
            {
                wr.UserAgent = useragent;
            }
            else
            {
                wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            }

            if (authorization != null)
            {
                wr.AddHeader("Authorization", authorization);
            }

            return wr.Get(url, null).ToString();
        }

        public static string postRequest(string url, string postdata, string contenttype, string useragent = null, string authorization = null)
        {
            HttpRequest wr = new HttpRequest();
            wr.Proxy = null;
            wr.KeepAlive = true;
            wr.IgnoreProtocolErrors = true;

            if (useragent != null)
            {
                wr.UserAgent = useragent;
            }
            else
            {
                wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            }

            if (authorization != null)
            {
                wr.AddHeader("Authorization", authorization);
            }

            return wr.Post(url, postdata, contenttype).ToString();
        }
    }
}
