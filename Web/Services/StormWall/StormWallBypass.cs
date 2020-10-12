using System;
using System.Net;

namespace Leaf.xNet.Services.StormWall
{
    /// <summary>
    /// Класс-расширение для обхода AntiDDoS защиты StormWall.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class StormWallBypass
    {
        #region Solver Singleton
        private static StormWallSolver _solver;
        private static StormWallSolver Solver => _solver ?? (_solver = new StormWallSolver());
        #endregion

        public static bool IsStormWalled(this HttpResponse rawResp)
        {
            return IsStormWalled(rawResp.ToString());
        }

        public static bool IsStormWalled(this string resp)
        {
            return resp.Contains("<h1>Stormwall DDoS protection</h1>")
                || resp.Contains("://reports.stormwall.pro");
        }

        // ReSharper disable once UnusedMember.Global
        public static HttpResponse GetThroughStormWall(this HttpRequest req, string url, HttpResponse rawResp)
        {
            return GetThroughStormWall(req, url, rawResp.ToString());
        }

        // TODO: loop with sleep (increasing delay sleep)
        /// <summary>
        /// Отправляет GET запрос, решая испытание StormWall.
        /// </summary>
        /// <param name="req">HTTP Запрос></param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="resp">Ответ со страницей StormWall если он был получен ранее. Чтобы не делать лишний запрос, оптимизация.</param>
        /// <returns>Вернет чистый ответ от сервера, без защиты Anti-DDoS.</returns>
        public static HttpResponse GetThroughStormWall(this HttpRequest req, string url, string resp = null)
        {
            if (resp == null)
            {
                var rawResp = req.Get(url);
                resp = rawResp.ToString();

                if (!resp.IsStormWalled())
                    return rawResp;
            }

            // Считываем параметры из ответа
            string cE = resp.ParseJsConstValue("cE");
            string cKstr = resp.ParseJsConstValue("cK", false);
            if (!int.TryParse(cKstr, out int cK))
                ThrowNotFoundJsValue("cK");

            string cN = resp.ParseJsConstValue("cN");
            string rpct = resp.ParseJsVariableValue("abc");

            // Инициализируем и решаем испытание
            Solver.Init(cE, cK, rpct);           
            string key = Solver.Solve();

            var cookie = new Cookie(cN, key, "/", new Uri(url).Host) {Expires = DateTime.Now.AddSeconds(30)};
            req.Cookies.Container.Add(cookie);

            string oldReferer = req.Referer;
            req.Referer = url;
            var respResult = req.Get(url);
            req.Referer = oldReferer;

            if (respResult.IsStormWalled())
                throw new StormWallException("Unable to pass StormWall at URL: " + url);

            return respResult;
        }

        private static void ThrowNotFoundJsValue(string variable)
        {
            throw new StormWallException($"Not found \"{variable}\" variable or const in StormWall code");
        }

        private static string ParseJsConstValue(this string resp, string variable, bool isString = true)
        {
            string patternBegin, patternEnd;

            if (isString)
            {
                patternBegin = "const {0} = \"";
                patternEnd = "\";";
            }
            else
            {
                patternBegin = "const {0} = ";
                patternEnd = ";";
            }

            string value = resp.Substring(string.Format(patternBegin, variable), patternEnd);
            if (value == null)
                ThrowNotFoundJsValue(variable);

            return value;
        }

        private static string ParseJsVariableValue(this string resp, string variable)
        {
            string value = resp.Substring($"var {variable}=\"", "\",");
            if (value == null)
                ThrowNotFoundJsValue(variable);

            return value;
        }
    }
}
