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
        //private readonly IConfidentialClientApplication _app;
        private static readonly string GraphApiEndpoint = "https://graph.microsoft.com/v1.0";
        private readonly string UserId = "cbf7a51b-d7b6-4dd9-897c-ce67dbae77f0";  // Use the provided OneDrive user ID.

        public OneDriveService(IOptions<OneDriveSettings> oneDriveSettings)
        {
            _oneDriveSettings = oneDriveSettings.Value;

            // Initialize MSAL client
            // _app = ConfidentialClientApplicationBuilder.Create(_oneDriveSettings.ClientId)
            //     .WithClientSecret(_oneDriveSettings.ClientSecret)
            //     .WithAuthority(new Uri($"https://login.microsoftonline.com/{_oneDriveSettings.TenantId}"))
            //     .Build();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var app = ConfidentialClientApplicationBuilder.Create(_oneDriveSettings.ClientId)
                .WithClientSecret(_oneDriveSettings.ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_oneDriveSettings.TenantId}"))
                .Build();
            var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
            return result.AccessToken;
        }

        public string ExtractDownloadUrlFromResponse(string responseBody)
        {
            var jsonObject = JObject.Parse(responseBody);
            var downloadUrl = jsonObject["@microsoft.graph.downloadUrl"].ToString();
            return downloadUrl;
        }

        public async Task<string> CreateUniqueFolderAsync()
        {
            var folderName = Guid.NewGuid().ToString();
            var createFolderPath = $"{GraphApiEndpoint}/users/{UserId}/drive/root/children";

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

                return folderName; // Return the name of the folder
            }
        }

        public async Task<string> UploadToOneDriveAsync(IFormFile file)
        {
            var folderName = await CreateUniqueFolderAsync();
            var fileName = file.FileName;
            var uploadPath = $"{GraphApiEndpoint}/users/{UserId}/drive/root:/{folderName}/{fileName}:/content";

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

                    // Here, we return the response body which will contain details of the uploaded file.
                    // You can further process this response if needed.
                    return ExtractDownloadUrlFromResponse(responseBody);
                }
            }
        }
    }
}
