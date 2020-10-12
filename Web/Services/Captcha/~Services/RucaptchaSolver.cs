using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading;

namespace Leaf.xNet.Services.Captcha
{
    // ReSharper disable once UnusedMember.Global
    /// <inheritdoc />
    public class RucaptchaSolver : BaseCaptchaSolver
    {
        public string Host { get; protected set; } = "rucaptcha.com";

        public CaptchaProxy Proxy { get; set; }

        public override string SolveRecaptcha(string pageUrl, string siteKey, CancellationToken cancelToken = default(CancellationToken))
        {
            // Validation
            ThrowIfApiKeyRequiredAndInvalid();

            if (string.IsNullOrEmpty(pageUrl))
                throw new ArgumentException($@"Invalid argument: ""{nameof(pageUrl)}"" = {pageUrl ?? "null"} when called ""{nameof(SolveRecaptcha)}""", nameof(pageUrl));
            
            if (string.IsNullOrEmpty(siteKey))
                throw new ArgumentException($@"Invalid argument: ""{nameof(siteKey)}"" = {siteKey ?? "null"} when called ""{nameof(SolveRecaptcha)}""", nameof(siteKey));

            // Upload
            //
            var postValues = new NameValueCollection {
                {"key", ApiKey},
                {"method", "userrecaptcha"},
                {"googlekey", siteKey},
                {"pageurl", pageUrl},
            };

            if (Proxy.IsValid)
            {
                postValues.Add("proxy", Proxy.Address);
                postValues.Add("proxytype", Proxy.Type.ToString());
            }
            
            string result = "unknown";
            bool fatalError = true;

            for (int i = 0; i < UploadRetries; i++)
            {
                cancelToken.ThrowIfCancellationRequested();
                result = Encoding.UTF8.GetString(Http.UploadValues($"http://{Host}/in.php", postValues));

                if (!result.Contains("ERROR_NO_SLOT_AVAILABLE"))
                {
                    fatalError = !result.Contains("OK|");
                    break;
                }

                Delay(UploadDelayOnNoSlotAvailable, cancelToken);
            }

            if (fatalError)
                throw new CaptchaException(result);

            // Status
            //
            string lastCaptchaId = result.Replace("OK|", "").Trim();

            fatalError = true;
            Delay(BeforeStatusCheckingDelay, cancelToken);

            for (int i = 0; i < StatusRetries; i++)
            {
                result = Http.DownloadString($"http://{Host}/res.php?key={ApiKey}&action=get&id={lastCaptchaId}");
                if (!result.Contains("CAPCHA_NOT_READY"))
                {
                    fatalError = !result.Contains("OK|");
                    break;
                }

                Delay(StatusDelayOnNotReady, cancelToken);
            }

            cancelToken.ThrowIfCancellationRequested();
            if (fatalError)
                throw new CaptchaException(result);

            string answer = result.Replace("OK|", "");
            if (string.IsNullOrEmpty(answer))
                throw new CaptchaException(CaptchaError.EmptyResponse);

            return answer;
        }


        #region Disabled

         
        /*
         public static class NewStringMethods
    {
        public static string ParseXml(this string str, string tag)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(tag)) 
                throw new Exception("Tag not found");

            string left = "<" + tag + ">";
            string right = "</" + tag + ">";

            // Ищем начало позиции левой подстроки.
            int leftPosBegin = str.IndexOf(left, 0, StringComparison.Ordinal);
            if (leftPosBegin == -1) 
                throw new Exception("Tag not found");
            // Вычисляем конец позиции левой подстроки.
            int leftPosEnd = leftPosBegin + left.Length;
            // Ищем начало позиции правой строки.
            int rightPos = str.IndexOf(right, leftPosEnd, StringComparison.Ordinal);
            if (rightPos == -1)
                throw new Exception("Tag not found");

            return str.Substring(leftPosEnd, rightPos - leftPosEnd);

        }
    }
    public struct RuCaptchaStats
    {
        public readonly int AvgTime;
        public readonly byte Load;
        public readonly float MinimalBid;
        public readonly int Waiting;

        public RuCaptchaStats(int avgTime, byte load, float minimalBid, int waiting)
        {
            AvgTime = avgTime;
            Load = load;
            MinimalBid = minimalBid;
            Waiting = waiting;
        }
    }
        string _lastCaptchaId;

        public string GetBalance()
        {
            string response;
            WebClient webClient = null;

            _cancel.ThrowIfCancellationRequested();

            try
            {
                webClient = new WebClient();
                response = webClient.DownloadString("http://" + Server + "/res.php?key=" + _key + "&action=getbalance");
            }
            catch
            {
                response = "Невозможно получить баланс Rucaptcha";
            }
            finally
            {
                webClient?.Dispose();
            }
            return response;
        }

        public static RuCaptchaStats GetStatistics()
        {
            string response;
            using (var webClient = new WebClient())
            {
                response = webClient.DownloadString("http://" + Server + "/load.php");
            }

            int avgTime = int.Parse(response.ParseXml("averageRecognitionTime"));
            byte load = byte.Parse(response.ParseXml("load"));
            float minimalBid = float.Parse(response.ParseXml("minbid").Replace('.',','));            
            int waiting = int.Parse(response.ParseXml("waiting"));            

            return new RuCaptchaStats(avgTime, load, minimalBid, waiting);
        }

        public string GetAllStatistics()
        {
            var stats = GetStatistics();
            return string.Format("Баланс RuCaptcha: {0} руб.{5}Bid: {1}; Load: {2}%; AVG: {3}; Waiting: {4}",
                GetBalance(), stats.MinimalBid, stats.Load, stats.AvgTime, stats.Waiting, Environment.NewLine);
        }

        public string Recognize(byte[] imageData, int minLength = 6, int maxLength = 6, bool onlyLetters = false, bool phrase = false, bool russian = false)
        {       
            // sending image
            var postValues = new NameValueCollection
            {
                { "key", _key },
                //{ "regsense", "0" },
                { "method", "base64" },
                { "body", Convert.ToBase64String(imageData) },
                { "min_len", minLength.ToString()},
                { "max_len", maxLength.ToString()},
                { "language", russian ? "1" : "2"},
                { "numeric", !onlyLetters ? "0" : "2"},
                { "phrase", !phrase ? "0" : "1"}
            };

            string result = "unknown";
            bool fatalError = true;

            for (int i = 0; i < TryCount; i++)
            {
                result = Encoding.UTF8.GetString(_webClient.UploadValues("http://" + Server + "/in.php", postValues));

                if (!result.Contains("ERROR_NO_SLOT_AVAILABLE"))
                {
                    fatalError = !result.Contains("OK|");
                    break;
                }

                _cancel.ThrowIfCancellationRequested();
                Thread.Sleep(WaitMsecBeforeRequest);
            }
            if (fatalError)
                throw new Exception("Ошибка загрузки RuCaptcha: " + result);

            _lastCaptchaId = result.Replace("OK|", "").Trim();

            fatalError = true;
            Thread.Sleep(WaitMsecBeforeRequest * 2);

            for (int i = 0; i < TryCountReady; i++)
            {
                result = _webClient.DownloadString("http://" + Server + "/res.php?key=" + _key + "&action=get&id=" + _lastCaptchaId);

                if (!result.Contains("CAPCHA_NOT_READY"))
                {
                    fatalError = !result.Contains("OK|");
                    break;
                }
                _cancel.ThrowIfCancellationRequested();
                Thread.Sleep(WaitMsecBeforeRequest);
            }

            _cancel.ThrowIfCancellationRequested();

            if (fatalError)
                throw new Exception("Ошибка распознавания RuCaptcha: " + result);

            return result.Replace("OK|", "").Trim();
        }

        public string ReportLastCaptcha()
        {
            return _webClient.DownloadString("http://" + Server + "/res.php?key=" + _key + "&action=reportbad&id=" + _lastCaptchaId);
        }

        public string Recognize(Image image, int minLength = 6, int maxLength = 6, bool onlyLetters = false, bool phrase = false, bool russian = false)
        {
            // convert image to array of bytes
            byte[] imageData;
            using (var stream = new MemoryStream())
            {
                image.Save(stream, image.RawFormat);
                imageData = stream.ToArray();
            }
            return Recognize(imageData, minLength, maxLength, onlyLetters, phrase, russian);
        }

        public string Recognize(string imageBase64)
        {
            // convert image to array of bytes
            
            //byte[] imageData;
            //using (var stream = new MemoryStream())
            //{
            //    image.Save(stream, image.RawFormat);
            //    imageData = stream.ToArray();
            //}
            
            // sending image
            var postValues = new NameValueCollection
            {
                { "key", _key },
                { "method", "base64" },
                { "body", imageBase64 },
                { "language", "1"},
                { "min_len", "5"},
                { "max_len", "6"}
            };

            string result = "unknown";
            bool fatalError = true;

            for (int i = 0; i < TryCount; i++)
            {
                result = Encoding.UTF8.GetString(_webClient.UploadValues("http://" + Server + "/in.php", postValues));

                if (!result.Contains("ERROR_NO_SLOT_AVAILABLE"))
                {
                    fatalError = !result.Contains("OK|");
                    break;
                }

                _cancel.WaitHandle.WaitOne(WaitMsecBeforeRequest);
            }
            if (fatalError)
                throw new Exception("Ошибка загрузки RuCaptcha: " + result);

            _lastCaptchaId = result.Replace("OK|", "").Trim();

            fatalError = true;

            _cancel.WaitHandle.WaitOne(WaitMsecBeforeRequest);

            for (int i = 0; i < TryCountReady; i++)
            {
                result = _webClient.DownloadString("http://" + Server + "/res.php?key=" + _key + "&action=get&id=" + _lastCaptchaId);

                if (!result.Contains("CAPCHA_NOT_READY"))
                {
                    fatalError = !result.Contains("OK|");
                    break;
                }

                _cancel.WaitHandle.WaitOne(WaitMsecBeforeRequest);
            }

            _cancel.ThrowIfCancellationRequested();
            if (fatalError)
                throw new CaptchaException("Ошибка распознавания RuCaptcha: " + result);
            
            return result.Replace("OK|", "").Trim().ToUpper();
        }*/
        #endregion
    }
}