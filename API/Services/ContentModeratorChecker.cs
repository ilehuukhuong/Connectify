using API.Helpers;
using API.Interfaces;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class ContentModeratorService : IContentModeratorService
    {
        private readonly ContentModeratorClient _client;

        public ContentModeratorService(IOptions<ContentModeratorSettings> config)
        {
            _client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(config.Value.Key))
            {
                Endpoint = config.Value.Endpoint
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