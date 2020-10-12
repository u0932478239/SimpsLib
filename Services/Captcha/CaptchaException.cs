using System;

namespace Leaf.xNet.Services.Captcha
{
    public enum CaptchaError
    {
        Unknown,
        CustomMessage,
        InvalidApiKey,
        EmptyResponse
    }

    //[Serializable]
    public class CaptchaException : Exception
    {
        public readonly CaptchaError Error;

        public override string Message => Error == CaptchaError.CustomMessage ? _message : Error.ToString();

        private readonly string _message;

        public CaptchaException(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Error = CaptchaError.Unknown;
                return;
            }
                
            _message = message;
            Error = CaptchaError.CustomMessage;
        }

        public CaptchaException(CaptchaError error)
        {
            Error = error;
        }
    }
}