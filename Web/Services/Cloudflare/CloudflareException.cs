using System;
// ReSharper disable UnusedMember.Global

namespace Leaf.xNet.Services.Cloudflare
{
    /// <inheritdoc />
    /// <summary>
    /// The exception that is thrown if Cloudflare clearance failed after the declared number of attempts.
    /// </summary>
    [Serializable]
    
    public class CloudflareException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Cloudflare solving exception.
        /// </summary>
        /// <param name="message">Message</param>
        public CloudflareException(string message) : base(message) { }

        public CloudflareException(string message, Exception inner) : base(message, inner) { }

        public CloudflareException(int attempts) : this(attempts, $"Clearance failed after {attempts} attempt(s).") { }

        public CloudflareException(int attempts, string message) : base(message)
        {
            Attempts = attempts;
        }

        public CloudflareException(int attempts, string message, Exception inner) : base(message, inner)
        {
            Attempts = attempts;
        }

        /// <summary>
        /// Returns the number of failed clearance attempts.
        /// </summary>
        public int Attempts { get; }
    }
}
