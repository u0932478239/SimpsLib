using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Security;
using System.Security;
#if IS_NETFRAMEWORK
using Microsoft.Win32;
using System.IO;
#endif

namespace Leaf.xNet
{
    /// <summary>
    /// Представляет статический класс, предназначенный для помощи в работе с HTTP-протоколом.
    /// </summary>
    public static class Http
    {
        #region Константы (открытые)

        /// <summary>
        /// Обозначает новую строку в HTTP-протоколе.
        /// </summary>
        public const string NewLine = "\r\n";

        /// <summary>
        /// Метод делегата, который принимает все сертификаты SSL.
        /// </summary>
        public static readonly RemoteCertificateValidationCallback AcceptAllCertificationsCallback;

        #endregion

        #region Статические поля (внутренние)

        internal static readonly Dictionary<HttpHeader, string> Headers = new Dictionary<HttpHeader, string>
        {
            { HttpHeader.Accept, "Accept" },
            { HttpHeader.AcceptCharset, "Accept-Charset" },
            { HttpHeader.AcceptLanguage, "Accept-Language" },
            { HttpHeader.AcceptDatetime, "Accept-Datetime" },
            { HttpHeader.CacheControl, "Cache-Control" },
            { HttpHeader.ContentType, "Content-Type" },
            { HttpHeader.Date, "Date" },
            { HttpHeader.Expect, "Expect" },
            { HttpHeader.From, "From" },
            { HttpHeader.IfMatch, "If-Match" },
            { HttpHeader.IfModifiedSince, "If-Modified-Since" },
            { HttpHeader.IfNoneMatch, "If-None-Match" },
            { HttpHeader.IfRange, "If-Range" },
            { HttpHeader.IfUnmodifiedSince, "If-Unmodified-Since" },
            { HttpHeader.MaxForwards, "Max-Forwards" },
            { HttpHeader.Pragma, "Pragma" },
            { HttpHeader.Range, "Range" },
            { HttpHeader.Referer, "Referer" },
            { HttpHeader.Origin, "Origin" },
            { HttpHeader.Upgrade, "Upgrade" },
            { HttpHeader.UpgradeInsecureRequests, "Upgrade-Insecure-Requests"},
            { HttpHeader.UserAgent, "User-Agent" },
            { HttpHeader.Via, "Via" },
            { HttpHeader.Warning, "Warning" },
            { HttpHeader.DNT, "DNT" },
            { HttpHeader.AccessControlAllowOrigin, "Access-Control-Allow-Origin" },
            { HttpHeader.AcceptRanges, "Accept-Ranges" },
            { HttpHeader.Age, "Age" },
            { HttpHeader.Allow, "Allow" },
            { HttpHeader.ContentEncoding, "Content-Encoding" },
            { HttpHeader.ContentLanguage, "Content-Language" },
            { HttpHeader.ContentLength, "Content-Length" },
            { HttpHeader.ContentLocation, "Content-Location" },
            { HttpHeader.ContentMD5, "Content-MD5" },
            { HttpHeader.ContentDisposition, "Content-Disposition" },
            { HttpHeader.ContentRange, "Content-Range" },
            { HttpHeader.ETag, "ETag" },
            { HttpHeader.Expires, "Expires" },
            { HttpHeader.LastModified, "Last-Modified" },
            { HttpHeader.Link, "Link" },
            { HttpHeader.Location, "Location" },
            { HttpHeader.P3P, "P3P" },
            { HttpHeader.Refresh, "Refresh" },
            { HttpHeader.RetryAfter, "Retry-After" },
            { HttpHeader.Server, "Server" },
            { HttpHeader.TransferEncoding, "Transfer-Encoding" }
        };

        #endregion

        static Http() => AcceptAllCertificationsCallback = AcceptAllCertifications;

        #region Статические методы (открытые)

        /// <summary>
        /// Преобразует параметры в строку запроса.
        /// </summary>
        /// <param name="parameters">Параметры.</param>
        /// <param name="valuesUnescaped">Указывает, нужно ли пропустить кодирование значений параметров запроса.</param>
        /// <param name="keysUnescaped">Указывает, нужно ли пропустить кодирование имен параметров запроса.</param>
        /// <returns>Строка запроса.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="parameters"/> равно <see langword="null"/>.</exception>
        // ReSharper disable once UnusedMember.Global
        public static string ToQueryString(IEnumerable<KeyValuePair<string, string>> parameters, 
            bool valuesUnescaped = false, bool keysUnescaped = false)
        {
            #region Проверка параметров

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            #endregion

            var queryBuilder = new StringBuilder();

            foreach (var param in parameters)
            {
                if (string.IsNullOrEmpty(param.Key))
                    continue;

                queryBuilder.Append(keysUnescaped ? param.Key : Uri.EscapeDataString(param.Key));
                queryBuilder.Append('=');

                queryBuilder.Append(valuesUnescaped ? param.Value : Uri.EscapeDataString(param.Value ?? string.Empty));

                queryBuilder.Append('&');
            }

            // Удаляем '&' в конце, если есть контент
            if (queryBuilder.Length != 0)
                queryBuilder.Remove(queryBuilder.Length - 1, 1);

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Определяет и возвращает MIME-тип на основе расширения файла.
        /// </summary>
        /// <param name="extension">Расширение файла.</param>
        /// <returns>MIME-тип.</returns>
        public static string DetermineMediaType(string extension)
        {
            string mediaType = "application/octet-stream";

            #if IS_NETFRAMEWORK
            try
            {                
                using (var regKey = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    var keyValue = regKey?.GetValue("Content Type");

                    if (keyValue != null)
                        mediaType = keyValue.ToString();
                }
                
            }
            #region Catch's

            catch (IOException) { }
            catch (ObjectDisposedException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }

            #endregion

            #else

            if (MimeTypes.ContainsKey(extension))
                mediaType = MimeTypes[extension];

            #endif

            return mediaType;
        }

        #region User Agent

        /// <summary>
        /// Генерирует случайный User-Agent от браузера IE.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера IE.</returns>
        public static string IEUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string version;
            string mozillaVersion;
            string trident;
            string otherParams;

            #region Генерация случайной версии

            if (windowsVersion.Contains("NT 5.1"))
            {
                version = "9.0";
                mozillaVersion = "5.0";
                trident = "5.0";
                otherParams = ".NET CLR 2.0.50727; .NET CLR 3.5.30729";
            }
            else if (windowsVersion.Contains("NT 6.0"))
            {
                version = "9.0";
                mozillaVersion = "5.0";
                trident = "5.0";
                otherParams = ".NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.30729";
            }
            else
            {
                switch (Randomizer.Instance.Next(3))
                {
                    case 0:
                        version = "10.0";
                        trident = "6.0";
                        mozillaVersion = "5.0";
                        break;

                    case 1:
                        version = "10.6";
                        trident = "6.0";
                        mozillaVersion = "5.0";
                        break;

                    default:
                        version = "11.0";
                        trident = "7.0";
                        mozillaVersion = "5.0";
                        break;
                }

                otherParams = ".NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E";
            }

            #endregion

            return
                $"Mozilla/{mozillaVersion} (compatible; MSIE {version}; {windowsVersion}; Trident/{trident}; {otherParams})";
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Opera.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Opera.</returns>
        public static string OperaUserAgent()
        {
            string version;
            string presto;

            #region Генерация случайной версии

            switch (Randomizer.Instance.Next(4))
            {
                case 0:
                    version = "12.16";
                    presto = "2.12.388";
                    break;

                case 1:
                    version = "12.14";
                    presto = "2.12.388";
                    break;

                case 2:
                    version = "12.02";
                    presto = "2.10.289";
                    break;

                default:
                    version = "12.00";
                    presto = "2.10.181";
                    break;
            }

            #endregion

            return $"Opera/9.80 ({RandomWindowsVersion()}); U) Presto/{presto} Version/{version}";
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Chrome.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Chrome.</returns>
        public static string ChromeUserAgent()
        {
            int major = Randomizer.Instance.Next(62, 70);
            int build = Randomizer.Instance.Next(2100, 3538);
            int branchBuild = Randomizer.Instance.Next(170);

            return $"Mozilla/5.0 ({RandomWindowsVersion()}) AppleWebKit/537.36 (KHTML, like Gecko) " +
                $"Chrome/{major}.0.{build}.{branchBuild} Safari/537.36";
        }


        private static readonly byte[] FirefoxVersions = { 64, 63, 62, 60, 58, 52, 51, 46, 45 };

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Firefox.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Firefox.</returns>
        public static string FirefoxUserAgent()
        {
            byte version = FirefoxVersions[Randomizer.Instance.Next(FirefoxVersions.Length - 1)];

            return $"Mozilla/5.0 ({RandomWindowsVersion()}; rv:{version}.0) Gecko/20100101 Firefox/{version}.0";
        }

        /// <summary>
        /// Генерирует случайный User-Agent от мобильного браузера Opera.
        /// </summary>
        /// <returns>Случайный User-Agent от мобильного браузера Opera.</returns>
        public static string OperaMiniUserAgent()
        {
            string os;
            string miniVersion;
            string version;
            string presto;

            #region Генерация случайной версии

            switch (Randomizer.Instance.Next(3))
            {
                case 0:
                    os = "iOS";
                    miniVersion = "7.0.73345";
                    version = "11.62";
                    presto = "2.10.229";
                    break;

                case 1:
                    os = "J2ME/MIDP";
                    miniVersion = "7.1.23511";
                    version = "12.00";
                    presto = "2.10.181";
                    break;

                default:
                    os = "Android";
                    miniVersion = "7.5.54678";
                    version = "12.02";
                    presto = "2.10.289";
                    break;
            }

            #endregion

            return $"Opera/9.80 ({os}; Opera Mini/{miniVersion}/28.2555; U; ru) Presto/{presto} Version/{version}";
        }

        /// <summary>
        /// Возвращает случайный User-Agent Chrome / Firefox / Opera основываясь на их популярности.
        /// </summary>
        /// <returns>Строка-значение заголовка User-Agent</returns>
        public static string RandomUserAgent()
        {
            int rand = Randomizer.Instance.Next(99) + 1;

            // TODO: edge, yandex browser, safari

            // Chrome = 70%
            if (rand >= 1 && rand <= 70)
                return ChromeUserAgent();

            // Firefox = 15%
            if (rand > 70 && rand <= 85)
                return FirefoxUserAgent();

            // IE = 6%
            if (rand > 85 && rand <= 91)
                return IEUserAgent();

            // Opera 12 = 5%
            if (rand > 91 && rand <= 96)
                return OperaUserAgent();

            // Opera mini = 4%
            return OperaMiniUserAgent();
        }

        #endregion

        #endregion


        #region Статические методы (закрытые)

        private static bool AcceptAllCertifications(object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certification,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            SslPolicyErrors sslPolicyErrors) => true;

        private static string RandomWindowsVersion()
        {
            string windowsVersion = "Windows NT ";
            int random = Randomizer.Instance.Next(99) + 1;

            // Windows 10 = 45% popularity
            if (random >= 1 && random <= 45)
                windowsVersion += "10.0";

            // Windows 7 = 35% popularity
            else if (random > 45 && random <= 80)
                windowsVersion += "6.1";

            // Windows 8.1 = 15% popularity
            else if (random > 80 && random <= 95)
                windowsVersion += "6.3";

            // Windows 8 = 5% popularity
            else
                windowsVersion += "6.2";

            // Append WOW64 for X64 system
            if (Randomizer.Instance.NextDouble() <= 0.65)
                windowsVersion += Randomizer.Instance.NextDouble() <= 0.5 ? "; WOW64" : "; Win64; x64";

            return windowsVersion;
        }

        #region Net Core mime types

        #if IS_NETCORE

        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string> {
            { ".323", "text/h323" },
            // { ".aaf", "application/octet-stream" },
            // { ".aca", "application/octet-stream" },
            { ".accdb", "application/msaccess" },
            { ".accde", "application/msaccess" },
            { ".accdt", "application/msaccess" },
            { ".acx", "application/internet-property-stream" },
            // { ".afm", "application/octet-stream" },
            { ".ai", "application/postscript" },
            { ".aif", "audio/x-aiff" },
            { ".aifc", "audio/aiff" },
            { ".aiff", "audio/aiff" },
            { ".application", "application/x-ms-application" },
            { ".art", "image/x-jg" },
            // { ".asd", "application/octet-stream" },
            { ".asf", "video/x-ms-asf" },
            // { ".asi", "application/octet-stream" },
            { ".asm", "text/plain" },
            { ".asr", "video/x-ms-asf" },
            { ".asx", "video/x-ms-asf" },
            { ".atom", "application/atom+xml" },
            { ".au", "audio/basic" },
            { ".avi", "video/x-msvideo" },
            { ".axs", "application/olescript" },
            { ".bas", "text/plain" },
            { ".bcpio", "application/x-bcpio" },
            // { ".bin", "application/octet-stream" },
            { ".bmp", "image/bmp" },
            { ".c", "text/plain" },
            // { ".cab", "application/octet-stream" },
            { ".calx", "application/vnd.ms-office.calx" },
            { ".cat", "application/vnd.ms-pki.seccat" },
            { ".cdf", "application/x-cdf" },
            // { ".chm", "application/octet-stream" },
            { ".class", "application/x-java-applet" },
            { ".clp", "application/x-msclip" },
            { ".cmx", "image/x-cmx" },
            { ".cnf", "text/plain" },
            { ".cod", "image/cis-cod" },
            { ".cpio", "application/x-cpio" },
            { ".cpp", "text/plain" },
            { ".crd", "application/x-mscardfile" },
            { ".crl", "application/pkix-crl" },
            { ".crt", "application/x-x509-ca-cert" },
            { ".csh", "application/x-csh" },
            { ".css", "text/css" },
            // { ".csv", "application/octet-stream" },
            // { ".cur", "application/octet-stream" },
            { ".dcr", "application/x-director" },
            // { ".deploy", "application/octet-stream" },
            { ".der", "application/x-x509-ca-cert" },
            { ".dib", "image/bmp" },
            { ".dir", "application/x-director" },
            { ".disco", "text/xml" },
            { ".dll", "application/x-msdownload" },
            { ".dll.config", "text/xml" },
            { ".dlm", "text/dlm" },
            { ".doc", "application/msword" },
            { ".docm", "application/vnd.ms-word.document.macroEnabled.12" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".dot", "application/msword" },
            { ".dotm", "application/vnd.ms-word.template.macroEnabled.12" },
            { ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
            // { ".dsp", "application/octet-stream" },
            { ".dtd", "text/xml" },
            { ".dvi", "application/x-dvi" },
            { ".dwf", "drawing/x-dwf" },
            // { ".dwp", "application/octet-stream" },
            { ".dxr", "application/x-director" },
            { ".eml", "message/rfc822" },
            // { ".emz", "application/octet-stream" },
            // { ".eot", "application/octet-stream" },
            { ".eps", "application/postscript" },
            { ".etx", "text/x-setext" },
            { ".evy", "application/envoy" },
            // { ".exe", "application/octet-stream" },
            { ".exe.config", "text/xml" },
            { ".fdf", "application/vnd.fdf" },
            { ".fif", "application/fractals" },
            // { ".fla", "application/octet-stream" },
            { ".flr", "x-world/x-vrml" },
            { ".flv", "video/x-flv" },
            { ".gif", "image/gif" },
            { ".gtar", "application/x-gtar" },
            { ".gz", "application/x-gzip" },
            { ".h", "text/plain" },
            { ".hdf", "application/x-hdf" },
            { ".hdml", "text/x-hdml" },
            { ".hhc", "application/x-oleobject" },
            // { ".hhk", "application/octet-stream" },
            // { ".hhp", "application/octet-stream" },
            { ".hlp", "application/winhlp" },
            { ".hqx", "application/mac-binhex40" },
            { ".hta", "application/hta" },
            { ".htc", "text/x-component" },
            { ".htm", "text/html" },
            { ".html", "text/html" },
            { ".htt", "text/webviewhtml" },
            { ".hxt", "text/html" },
            { ".ico", "image/x-icon" },
            // { ".ics", "application/octet-stream" },
            { ".ief", "image/ief" },
            { ".iii", "application/x-iphone" },
            // { ".inf", "application/octet-stream" },
            { ".ins", "application/x-internet-signup" },
            { ".isp", "application/x-internet-signup" },
            { ".IVF", "video/x-ivf" },
            { ".jar", "application/java-archive" },
            // { ".java", "application/octet-stream" },
            { ".jck", "application/liquidmotion" },
            { ".jcz", "application/liquidmotion" },
            { ".jfif", "image/pjpeg" },
            // { ".jpb", "application/octet-stream" },
            { ".jpe", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".js", "application/x-javascript" },
            { ".jsx", "text/jscript" },
            { ".latex", "application/x-latex" },
            { ".lit", "application/x-ms-reader" },
            // { ".lpk", "application/octet-stream" },
            { ".lsf", "video/x-la-asf" },
            { ".lsx", "video/x-la-asf" },
            // { ".lzh", "application/octet-stream" },
            { ".m13", "application/x-msmediaview" },
            { ".m14", "application/x-msmediaview" },
            { ".m1v", "video/mpeg" },
            { ".m3u", "audio/x-mpegurl" },
            { ".man", "application/x-troff-man" },
            { ".manifest", "application/x-ms-manifest" },
            { ".map", "text/plain" },
            { ".mdb", "application/x-msaccess" },
            // { ".mdp", "application/octet-stream" },
            { ".me", "application/x-troff-me" },
            { ".mht", "message/rfc822" },
            { ".mhtml", "message/rfc822" },
            { ".mid", "audio/mid" },
            { ".midi", "audio/mid" },
            // { ".mix", "application/octet-stream" },
            { ".mmf", "application/x-smaf" },
            { ".mno", "text/xml" },
            { ".mny", "application/x-msmoney" },
            { ".mov", "video/quicktime" },
            { ".movie", "video/x-sgi-movie" },
            { ".mp2", "video/mpeg" },
            { ".mp3", "audio/mpeg" },
            { ".mpa", "video/mpeg" },
            { ".mpe", "video/mpeg" },
            { ".mpeg", "video/mpeg" },
            { ".mpg", "video/mpeg" },
            { ".mpp", "application/vnd.ms-project" },
            { ".mpv2", "video/mpeg" },
            { ".ms", "application/x-troff-ms" },
            // { ".msi", "application/octet-stream" },
            // { ".mso", "application/octet-stream" },
            { ".mvb", "application/x-msmediaview" },
            { ".mvc", "application/x-miva-compiled" },
            { ".nc", "application/x-netcdf" },
            { ".nsc", "video/x-ms-asf" },
            { ".nws", "message/rfc822" },
            // { ".ocx", "application/octet-stream" },
            { ".oda", "application/oda" },
            { ".odc", "text/x-ms-odc" },
            { ".ods", "application/oleobject" },
            { ".one", "application/onenote" },
            { ".onea", "application/onenote" },
            { ".onetoc", "application/onenote" },
            { ".onetoc2", "application/onenote" },
            { ".onetmp", "application/onenote" },
            { ".onepkg", "application/onenote" },
            { ".osdx", "application/opensearchdescription+xml" },
            { ".p10", "application/pkcs10" },
            { ".p12", "application/x-pkcs12" },
            { ".p7b", "application/x-pkcs7-certificates" },
            { ".p7c", "application/pkcs7-mime" },
            { ".p7m", "application/pkcs7-mime" },
            { ".p7r", "application/x-pkcs7-certreqresp" },
            { ".p7s", "application/pkcs7-signature" },
            { ".pbm", "image/x-portable-bitmap" },
            // { ".pcx", "application/octet-stream" },
            // { ".pcz", "application/octet-stream" },
            { ".pdf", "application/pdf" },
            // { ".pfb", "application/octet-stream" },
            // { ".pfm", "application/octet-stream" },
            { ".pfx", "application/x-pkcs12" },
            { ".pgm", "image/x-portable-graymap" },
            { ".pko", "application/vnd.ms-pki.pko" },
            { ".pma", "application/x-perfmon" },
            { ".pmc", "application/x-perfmon" },
            { ".pml", "application/x-perfmon" },
            { ".pmr", "application/x-perfmon" },
            { ".pmw", "application/x-perfmon" },
            { ".png", "image/png" },
            { ".pnm", "image/x-portable-anymap" },
            { ".pnz", "image/png" },
            { ".pot", "application/vnd.ms-powerpoint" },
            { ".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
            { ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
            { ".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" },
            { ".ppm", "image/x-portable-pixmap" },
            { ".pps", "application/vnd.ms-powerpoint" },
            { ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
            { ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".prf", "application/pics-rules" },
            // { ".prm", "application/octet-stream" },
            // { ".prx", "application/octet-stream" },
            { ".ps", "application/postscript" },
            // { ".psd", "application/octet-stream" },
            // { ".psm", "application/octet-stream" },
            // { ".psp", "application/octet-stream" },
            { ".pub", "application/x-mspublisher" },
            { ".qt", "video/quicktime" },
            { ".qtl", "application/x-quicktimeplayer" },
            // { ".qxd", "application/octet-stream" },
            { ".ra", "audio/x-pn-realaudio" },
            { ".ram", "audio/x-pn-realaudio" },
            // { ".rar", "application/octet-stream" },
            { ".ras", "image/x-cmu-raster" },
            { ".rf", "image/vnd.rn-realflash" },
            { ".rgb", "image/x-rgb" },
            { ".rm", "application/vnd.rn-realmedia" },
            { ".rmi", "audio/mid" },
            { ".roff", "application/x-troff" },
            { ".rpm", "audio/x-pn-realaudio-plugin" },
            { ".rtf", "application/rtf" },
            { ".rtx", "text/richtext" },
            { ".scd", "application/x-msschedule" },
            { ".sct", "text/scriptlet" },
            // { ".sea", "application/octet-stream" },
            { ".setpay", "application/set-payment-initiation" },
            { ".setreg", "application/set-registration-initiation" },
            { ".sgml", "text/sgml" },
            { ".sh", "application/x-sh" },
            { ".shar", "application/x-shar" },
            { ".sit", "application/x-stuffit" },
            { ".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12" },
            { ".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
            { ".smd", "audio/x-smd" },
            // { ".smi", "application/octet-stream" },
            { ".smx", "audio/x-smd" },
            { ".smz", "audio/x-smd" },
            { ".snd", "audio/basic" },
            // { ".snp", "application/octet-stream" },
            { ".spc", "application/x-pkcs7-certificates" },
            { ".spl", "application/futuresplash" },
            { ".src", "application/x-wais-source" },
            { ".ssm", "application/streamingmedia" },
            { ".sst", "application/vnd.ms-pki.certstore" },
            { ".stl", "application/vnd.ms-pki.stl" },
            { ".sv4cpio", "application/x-sv4cpio" },
            { ".sv4crc", "application/x-sv4crc" },
            { ".swf", "application/x-shockwave-flash" },
            { ".t", "application/x-troff" },
            { ".tar", "application/x-tar" },
            { ".tcl", "application/x-tcl" },
            { ".tex", "application/x-tex" },
            { ".texi", "application/x-texinfo" },
            { ".texinfo", "application/x-texinfo" },
            { ".tgz", "application/x-compressed" },
            { ".thmx", "application/vnd.ms-officetheme" },
            // { ".thn", "application/octet-stream" },
            { ".tif", "image/tiff" },
            { ".tiff", "image/tiff" },
            // { ".toc", "application/octet-stream" },
            { ".tr", "application/x-troff" },
            { ".trm", "application/x-msterminal" },
            { ".tsv", "text/tab-separated-values" },
            // { ".ttf", "application/octet-stream" },
            { ".txt", "text/plain" },
            // { ".u32", "application/octet-stream" },
            { ".uls", "text/iuls" },
            { ".ustar", "application/x-ustar" },
            { ".vbs", "text/vbscript" },
            { ".vcf", "text/x-vcard" },
            { ".vcs", "text/plain" },
            { ".vdx", "application/vnd.ms-visio.viewer" },
            { ".vml", "text/xml" },
            { ".vsd", "application/vnd.visio" },
            { ".vss", "application/vnd.visio" },
            { ".vst", "application/vnd.visio" },
            { ".vsto", "application/x-ms-vsto" },
            { ".vsw", "application/vnd.visio" },
            { ".vsx", "application/vnd.visio" },
            { ".vtx", "application/vnd.visio" },
            { ".wav", "audio/wav" },
            { ".wax", "audio/x-ms-wax" },
            { ".wbmp", "image/vnd.wap.wbmp" },
            { ".wcm", "application/vnd.ms-works" },
            { ".wdb", "application/vnd.ms-works" },
            { ".wks", "application/vnd.ms-works" },
            { ".wm", "video/x-ms-wm" },
            { ".wma", "audio/x-ms-wma" },
            { ".wmd", "application/x-ms-wmd" },
            { ".wmf", "application/x-msmetafile" },
            { ".wml", "text/vnd.wap.wml" },
            { ".wmlc", "application/vnd.wap.wmlc" },
            { ".wmls", "text/vnd.wap.wmlscript" },
            { ".wmlsc", "application/vnd.wap.wmlscriptc" },
            { ".wmp", "video/x-ms-wmp" },
            { ".wmv", "video/x-ms-wmv" },
            { ".wmx", "video/x-ms-wmx" },
            { ".wmz", "application/x-ms-wmz" },
            { ".wps", "application/vnd.ms-works" },
            { ".wri", "application/x-mswrite" },
            { ".wrl", "x-world/x-vrml" },
            { ".wrz", "x-world/x-vrml" },
            { ".wsdl", "text/xml" },
            { ".wvx", "video/x-ms-wvx" },
            { ".x", "application/directx" },
            { ".xaf", "x-world/x-vrml" },
            { ".xaml", "application/xaml+xml" },
            { ".xap", "application/x-silverlight-app" },
            { ".xbap", "application/x-ms-xbap" },
            { ".xbm", "image/x-xbitmap" },
            { ".xdr", "text/plain" },
            { ".xla", "application/vnd.ms-excel" },
            { ".xlam", "application/vnd.ms-excel.addin.macroEnabled.12" },
            { ".xlc", "application/vnd.ms-excel" },
            { ".xlm", "application/vnd.ms-excel" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
            { ".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".xlt", "application/vnd.ms-excel" },
            { ".xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
            { ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
            { ".xlw", "application/vnd.ms-excel" },
            { ".xml", "text/xml" },
            { ".xof", "x-world/x-vrml" },
            { ".xpm", "image/x-xpixmap" },
            { ".xps", "application/vnd.ms-xpsdocument" },
            { ".xsd", "text/xml" },
            { ".xsf", "text/xml" },
            { ".xsl", "text/xml" },
            { ".xslt", "text/xml" },
            // { ".xsn", "application/octet-stream" },
            // { ".xtp", "application/octet-stream" },
            { ".xwd", "image/x-xwindowdump" },
            { ".z", "application/x-compress" },
            { ".zip", "application/x-zip-compressed" }
        };
        #endif

        #endregion

        #endregion
    }
}
