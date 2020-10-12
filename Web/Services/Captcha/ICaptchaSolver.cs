using System;
using System.IO;
using System.Threading;

namespace Leaf.xNet.Services.Captcha
{
    public interface ICaptchaSolver
    {
        uint UploadRetries { get; set; }
        uint StatusRetries { get; set; }

        TimeSpan UploadDelayOnNoSlotAvailable { get; set; }
        TimeSpan StatusDelayOnNotReady { get; set; }
        TimeSpan BeforeStatusCheckingDelay { get; set; }

        string SolveImage(string imageUrl, CancellationToken cancelToken = default(CancellationToken));
        string SolveImage(byte[] imageBytes, CancellationToken cancelToken = default(CancellationToken));
        string SolveImage(Stream imageStream, CancellationToken cancelToken = default(CancellationToken));
        string SolveImageFromBase64(string imageBase64, CancellationToken cancelToken = default(CancellationToken));

        string SolveRecaptcha(string pageUrl, string siteKey, CancellationToken cancelToken = default(CancellationToken));
    }
}