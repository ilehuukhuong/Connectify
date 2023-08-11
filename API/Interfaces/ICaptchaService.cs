public interface ICaptchaService
{
    Task<bool> VerifyCaptcha(string captchaResponse);
}