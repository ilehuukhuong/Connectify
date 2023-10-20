using API.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public CaptchaService(IHttpClientFactory httpClientFactory, IOptions<ReCaptchaSettings> reCaptchaSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _secretKey = reCaptchaSettings.Value.SecretKey;
        }

        public async Task<bool> VerifyCaptcha(string captchaResponse)
        {
            var response = await _httpClient.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_secretKey}&response={captchaResponse}", null);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var captchaVerifyResponse = JsonConvert.DeserializeObject<CaptchaVerifyResponse>(jsonResponse);
            return captchaVerifyResponse.Success;
        }

        private class CaptchaVerifyResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
        }
    }
}