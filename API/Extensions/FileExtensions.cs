namespace API.Extensions
{
    public static class FileExtensions
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif",".bmp",".tiff",".tif",".webp",".svg",".raw",".psd",".heic",".ai",".eps" };
        private static readonly string[] AllowedVideoExtensions = {".mp4",".avi",".mkv",".mov",".flv",".wmv",".m4v",".webm",".3gp",".mpeg",".mpg",".vob",".rm",".rmvb",".ts",".ogv",".swf"};
        private const int MaxImageSizeInBytes = 25 * 1024 * 1024; // 25 MB
        private const int MaxVideoSizeInBytes = 500 * 1024 * 1024; // 500 MB

        public static bool IsImage(this IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var extension = Path.GetExtension(file.FileName).ToLower();

            return AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && file.Length <= MaxImageSizeInBytes;
        }

        public static bool IsVideo(this IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var extension = Path.GetExtension(file.FileName).ToLower();

            return AllowedVideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && file.Length <= MaxVideoSizeInBytes;
        }
    }
}