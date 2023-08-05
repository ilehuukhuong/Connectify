using API.Interfaces;
using Microsoft.Azure.CognitiveServices.ContentModerator;

namespace API.Services
{
        public class ContentModeratorService : IContentModeratorService
    {
        private readonly ContentModeratorClient _client;

        public ContentModeratorService(IConfiguration configuration)
        {
            string subscriptionKey = configuration["ContentModerator:SubscriptionKey"];
            string endpoint = configuration["ContentModerator:Endpoint"];
            _client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint
            };
        }

        public async Task<bool> IsInappropriateText(string text)
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
            var screenResult = await _client.TextModeration.ScreenTextAsync("text/plain", stream, language: "eng");

            return screenResult.Terms?.Count > 0;
        }
    }
}