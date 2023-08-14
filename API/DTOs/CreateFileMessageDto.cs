using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateFileMessageDto
    {
        public string RecipientUsername { get; set; }
        [Required]
        public string File64 { get; set; }
        [Required]
        public string FileName { get; set; }
        public IFormFile File { get; set; }

        public void ConvertFile64ToIFormFile()
        {
            byte[] fileBytes = Convert.FromBase64String(File64);
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, fileBytes.Length, "File", FileName);
            File = formFile;
        }
    }
}