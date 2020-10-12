using System;
using System.Diagnostics.CodeAnalysis;

namespace Leaf.xNet.Services.Captcha
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum CaptchaProxyType
    {
        HTTP,
        HTTPS,
        SOCKS4,
        SOCKS5
    }

    public struct CaptchaProxy
    {
        public readonly CaptchaProxyType Type;
        public readonly string Address;

        public bool IsValid => !Equals(this, default(CaptchaProxy)) && !string.IsNullOrEmpty(Address);

        public CaptchaProxy(CaptchaProxyType type, string address)
        {
            Validate(type, address);

            Type = type;
            Address = address;
        }

        public CaptchaProxy(string type, string address)
        {
            bool typeIsValid = Enum.TryParse(type.Trim().ToUpper(), out CaptchaProxyType proxyType);
            if (!typeIsValid)
                throw new ArgumentException(@"Proxy type is invalid. Available: HTTP, HTTPS, SOCKS4, SOCKS5", nameof(address));

            Validate(proxyType, address);

            Type = proxyType;
            Address = address;
        }

        private static void Validate(CaptchaProxyType type, string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException($@"{nameof(CaptchaProxy)} should contain {nameof(address)}", nameof(address));

            // Simple validate port
            int portIndex = address.IndexOf(':');
            const int minPortLength = 2;

            if (portIndex == -1 || address.Length - 1 - portIndex < minPortLength)
                throw new ArgumentException($@"{nameof(address)} should contain port", nameof(address));
        }
    }
}