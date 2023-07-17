using API.Helpers;
using API.Interfaces;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class NSFWChecker : INSFWChecker
    {
        private readonly ComputerVisionClient _computerVisionClient;

        public NSFWChecker(IOptions<ComputervisionSettings> config)
        {
            _computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(config.Value.Key))
            {
                Endpoint = config.Value.Endpoint
            };
        }

        public async Task<bool> IsNSFWPhoto(IFormFile file)
        {
            using var photoStream = file.OpenReadStream();

            var result = await _computerVisionClient.AnalyzeImageInStreamAsync(photoStream, new List<VisualFeatureTypes?> { VisualFeatureTypes.Adult });

            if (result?.Adult != null && (result.Adult.IsAdultContent || result.Adult.IsRacyContent))
            {
                return true; // Photo contains NSFW content
            }

            return false; // Photo does not contain NSFW content
        }
    }
}