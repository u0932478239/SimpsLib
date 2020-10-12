using System;
using System.Globalization;

namespace Leaf.xNet.Services.Cloudflare
{
    /// <inheritdoc cref="T:IEquatable" />
    /// <summary>
    /// Содержит информацию которая необходима для прохождения испытания CloudFlare.
    /// </summary>
    public struct ChallengeSolution : IEquatable<ChallengeSolution>
    {
        /// <summary>
        /// Адрес страницы где необходимо пройти испытание.
        /// </summary>
        public string ClearancePage { get; }

        /// <summary>
        /// Код верификации.
        /// </summary>
        public string VerificationCode { get; }

        /// <summary>
        /// Вхождение.
        /// </summary>
        public string Pass { get; }

        /// <summary>
        /// Ответ на JS Challenge.
        /// </summary>
        public double Answer { get; }

        /// <summary>
        /// Новый модификатор JS Challenge.
        /// </summary>
        public string S { get; }

        /// <summary>
        /// Вернет истину если испытание подсчитывается только типом <see cref="Int32"/>, а не <see cref="Double"/> с плавающей точкой.
        /// </summary>
        public bool ContainsIntegerTag { get; }

        /// <summary>
        /// Результирующий URL запроса который необходимо исполнить для прохождения JS испытания.
        /// </summary>
        public string ClearanceQuery => !(string.IsNullOrEmpty(S)) ?
            $"{ClearancePage}?s={Uri.EscapeDataString(S)}&jschl_vc={VerificationCode}&pass={Pass}&jschl_answer={Answer.ToString("R", CultureInfo.InvariantCulture)}" :
            $"{ClearancePage}?jschl_vc={VerificationCode}&pass={Pass}&jschl_answer={Answer.ToString("R", CultureInfo.InvariantCulture)}";

        /// <summary>
        /// Содержит информацию которая необходима для прохождения испытания CloudFlare.
        /// </summary>
        public ChallengeSolution(string clearancePage, string verificationCode, string pass, double answer, string s,
            bool containsIntegerTag)
        {
            ClearancePage = clearancePage;
            VerificationCode = verificationCode;
            Pass = pass;
            Answer = answer;
            S = s;
            ContainsIntegerTag = containsIntegerTag;
        }

        /// <summary>
        /// Выполняет сравнение "РАВНО" для <see cref="ChallengeSolution"/>.
        /// </summary>
        /// <returns>Вернет истину если результаты равны</returns>
        public static bool operator ==(ChallengeSolution solutionA, ChallengeSolution solutionB)
        {
            return solutionA.Equals(solutionB);
        }

        /// <summary>
        /// Выполняет сравнение "Не РАВНО" для <see cref="ChallengeSolution"/>.
        /// </summary>
        /// <returns>Вернет истину если результаты не равны</returns>
        public static bool operator !=(ChallengeSolution solutionA, ChallengeSolution solutionB)
        {
            return !(solutionA == solutionB);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ChallengeSolution other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ClearanceQuery.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(ChallengeSolution other)
        {
            return other.ClearanceQuery == ClearanceQuery;
        }
    }
}