using System.Net.Http.Headers;
using System.Text;
using API.Helpers;
using API.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Services
{
    public class OneDriveService : IOneDriveService
    {
        private readonly OneDriveSettings _oneDriveSettings;
        private readonly IConfidentialClientApplication _app;
        private static readonly string GraphApiEndpoint = "https://graph.microsoft.com/v1.0";

        public OneDriveService(IOptions<OneDriveSettings> oneDriveSettings)
        {
            _oneDriveSettings = oneDriveSettings.Value;

            // Initialize MSAL client
            _app = ConfidentialClientApplicationBuilder.Create(_oneDriveSettings.ClientId)
                .WithClientSecret(_oneDriveSettings.ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_oneDriveSettings.TenantId}"))
                .Build();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var result = await _app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
            return result.AccessToken;
        }

        public string ExtractIdFromResponse(string responseBody)
        {
            var jsonObject = JObject.Parse(responseBody);
            var fileId = jsonObject["id"].ToString();
            return fileId;
        }

        public async Task<string> CreateUniqueFolderAsync()
        {
            var folderName = Guid.NewGuid().ToString();
            // The line below is for the case of saving to a folder
            // var createFolderPath = $"{GraphApiEndpoint}/users/{_oneDriveSettings.UserId}/drive/root:/Connectify:/children";
            var createFolderPath = $"{GraphApiEndpoint}/users/{_oneDriveSettings.UserId}/drive/root/children";

            var folderContent = new JObject
            {
                ["name"] = folderName,
                ["folder"] = new JObject(),
                ["@microsoft.graph.conflictBehavior"] = "rename"
            };

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());

                var response = await httpClient.PostAsync(createFolderPath, new StringContent(JsonConvert.SerializeObject(folderContent), Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create folder. Status code: {response.StatusCode}. Error: {responseBody}");
                }

                return folderName;
            }
        }

        public async Task<string> UploadToOneDriveAsync(IFormFile file)
        {
            var folderName = await CreateUniqueFolderAsync();
            var fileName = file.FileName;
            var uploadPath = $"{GraphApiEndpoint}/users/{_oneDriveSettings.UserId}/drive/root:/{folderName}/{fileName}:/content";
            // The line below is for the case of saving to a folder
            // var uploadPath = $"{GraphApiEndpoint}/users/{_oneDriveSettings.UserId}/drive/root:/Connectify/{folderName}/{fileName}:/content";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());

                using (var content = new StreamContent(file.OpenReadStream()))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // POST request to upload the file
                    var response = await httpClient.PutAsync(uploadPath, content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to upload file. Status code: {response.StatusCode}. Error: {responseBody}");
                    }

                    return ExtractIdFromResponse(responseBody);
                }
            }
        }

        public async Task<string> BuildDownloadUrl(string fileId)
        {
            var downloadUrl = $"{GraphApiEndpoint}/drives/{_oneDriveSettings.UserId}/items/{fileId}/content";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync());

                var response = httpClient.GetAsync(downloadUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var requestUri = response.RequestMessage.RequestUri.ToString();
                    return requestUri;
                }
                else
                {
                    throw new Exception($"Failed to build download URL. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
