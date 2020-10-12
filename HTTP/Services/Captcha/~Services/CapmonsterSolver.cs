namespace SimpsLib.HTTP.Services.Captcha
{
    public class CapmonsterSolver : RucaptchaSolver
    {
        public CapmonsterSolver(string host = "127.0.0.3:80")
        {
            Host = host;
            IsApiKeyRequired = false;
        }
    }
}