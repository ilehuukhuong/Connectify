using API.Helpers;
using API.Interfaces;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace API.Services
{
    public class ContentModeratorService : IContentModeratorService
    {
        private readonly ContentModeratorClient _client;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public ContentModeratorService(IOptions<ContentModeratorSettings> config)
        {
            _client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(config.Value.Key))
            {
                Endpoint = config.Value.Endpoint
            };
            _apiKey = config.Value.Key;
            _baseUrl = config.Value.Endpoint;
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
        }

        public async Task<bool> IsInappropriateText(string text)
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
            var screenResult = await _client.TextModeration.ScreenTextAsync("text/plain", stream, language: "eng");

            return screenResult.Terms?.Count > 0;
        }

        public async Task<string> GetAllTermListsAsync()
        {
            var url = $"{_baseUrl}contentmoderator/lists/v1.0/termlists";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<string> CreateTermListAsync(string name, string description)
        {
            var url = $"{_baseUrl}contentmoderator/lists/v1.0/termlists";
            var body = new { Name = name, Description = description };
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task AddTermToListAsync(string listId, string term, string language = "eng")
        {
            var url = $"{_baseUrl}contentmoderator/lists/v1.0/termlists/{listId}/terms/{term}?language={language}";
            var response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTermFromListAsync(string listId, string term, string language = "eng")
        {
            var url = $"{_baseUrl}contentmoderator/lists/v1.0/termlists/{listId}/terms/{term}?language={language}";
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        public async Task RefreshTermListAsync(string listId)
        {
            var url = $"{_baseUrl}contentmoderator/lists/v1.0/termlists/{listId}/RefreshIndex?language=eng";
            var response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTermListAsync(string listId)
        {
            var url = $"{_baseUrl}contentmoderator/lists/v1.0/termlists/{listId}";
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetAllTermsInTermListAsync(string listId, string language = "eng")
        {
            var url = $"{_baseUrl}contentmoderator/lists/v1.0/termlists/{listId}/terms?language={language}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}