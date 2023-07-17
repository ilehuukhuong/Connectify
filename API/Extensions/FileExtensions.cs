namespace API.Extensions
{
    public static class FileExtensions
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        public static bool IsImage(this IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var extension = Path.GetExtension(file.FileName);

            return AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && file.Length <= MaxFileSizeInBytes;
        }
    }
}