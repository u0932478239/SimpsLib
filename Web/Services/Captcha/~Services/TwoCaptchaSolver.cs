namespace Leaf.xNet.Services.Captcha
{
    // ReSharper disable once UnusedMember.Global
    /// <inheritdoc />
    public class TwoCaptchaSolver : RucaptchaSolver
    {
        public TwoCaptchaSolver()
        {
            Host = "2captcha.com";
        }
    }
}