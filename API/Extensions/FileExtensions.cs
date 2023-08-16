namespace API.Extensions
{
    public static class FileExtensions
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".svg", ".raw", ".psd", ".heic", ".ai", ".eps" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".flv", ".wmv", ".m4v", ".webm", ".3gp", ".mpeg", ".mpg", ".vob", ".rm", ".rmvb", ".ts", ".ogv", ".swf" };
        private static readonly string[] AllowedAudioExtensions = { ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".flac", ".wma", ".alac", ".ac3", ".amr", ".dts", ".ra", ".mid", ".midi", ".ape", ".aiff", ".mka" };
        private const int MaxImageSizeInBytes = 50 * 1024 * 1024; // 50 MB
        private const int MaxAudioSizeInBytes = 500 * 1024 * 1024; // 500 MB
        private const int MaxVideoSizeInBytes = 500 * 1024 * 1024; // 500 MB

        public static bool IsImage(this IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var extension = Path.GetExtension(file.FileName).ToLower();

            return AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && file.Length <= MaxImageSizeInBytes;
        }

        public static bool IsAudio(this IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var extension = Path.GetExtension(file.FileName).ToLower();

            return AllowedAudioExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && file.Length <= MaxAudioSizeInBytes;
        }

        public static bool IsVideo(this IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var extension = Path.GetExtension(file.FileName).ToLower();

            return AllowedVideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && file.Length <= MaxVideoSizeInBytes;
        }

        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Length > 255)
                return false;

            // Danh sách các ký tự không hợp lệ trong tên file
            char[] invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

            return fileName.All(ch => !invalidChars.Contains(ch));
        }
    }
}