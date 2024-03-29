namespace API.Interfaces
{
    public interface IOneDriveService
    {
        Task<string> UploadToOneDriveAsync(IFormFile file);
        Task<string> BuildDownloadUrl(string fileId);
    }
}