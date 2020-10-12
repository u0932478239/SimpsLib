using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Leaf.xNet.Services.Captcha
{
    public abstract class BaseCaptchaSolver : ICaptchaSolver, IDisposable
    {
        public string ApiKey { get; set; }
        public bool IsApiKeyRequired { get; protected set; } = true;
        public virtual bool IsApiKeyValid => !string.IsNullOrEmpty(ApiKey);
        
        public uint UploadRetries { get; set; } = 40;
        public uint StatusRetries { get; set; } = 80;

        public TimeSpan UploadDelayOnNoSlotAvailable { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan StatusDelayOnNotReady { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan BeforeStatusCheckingDelay { get; set; } = TimeSpan.FromSeconds(3);

        public const string NameOfString = "string";

        protected readonly AdvancedWebClient Http = new AdvancedWebClient();

        #region SolveImage : Generic

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveImage(string imageUrl, CancellationToken cancelToken = default(CancellationToken))
        {
            throw NotImplemented(nameof(SolveImage), NameOfString);
        }

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveImage(byte[] imageBytes, CancellationToken cancelToken = default(CancellationToken))
        {
            throw NotImplemented(nameof(SolveImage), "byte[]");
        }

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveImage(Stream imageStream, CancellationToken cancelToken = default(CancellationToken))
        {
            throw NotImplemented(nameof(SolveImage), nameof(Stream));
        }

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public string SolveImageFromBase64(string imageBase64, CancellationToken cancelToken = default(CancellationToken))
        {
            throw NotImplemented(nameof(SolveImageFromBase64), NameOfString);
        }

        #endregion

        
        #region SolveImage : Recaptcha

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveRecaptcha(string pageUrl, string siteKey, CancellationToken cancelToken = default(CancellationToken))
        {
            throw NotImplemented(nameof(SolveRecaptcha), "string, string");
        }

        #endregion

        protected void ThrowIfApiKeyRequiredAndInvalid()
        {
            if (IsApiKeyRequired && !IsApiKeyValid)
                throw new CaptchaException(CaptchaError.InvalidApiKey);
        }

        protected void Delay(TimeSpan delay, CancellationToken cancelToken)
        {
            if (cancelToken != CancellationToken.None)
            {
                cancelToken.WaitHandle.WaitOne(UploadDelayOnNoSlotAvailable);
                cancelToken.ThrowIfCancellationRequested();
            }
            else
                Thread.Sleep(UploadDelayOnNoSlotAvailable);
        }

        private NotImplementedException NotImplemented(string method, string parameterType)
        {
            return new NotImplementedException($"Method \"{method}\"({parameterType}) of {GetType().Name} isn't implemented");
        }

        #region IDisposable

        public virtual void Dispose()
        {
            Http?.Dispose();
        }
        
        #endregion     
    }
}