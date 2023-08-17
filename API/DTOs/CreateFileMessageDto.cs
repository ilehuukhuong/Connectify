using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateFileMessageDto
    {
        public string RecipientUsername { get; set; }
        public IFormFile File { get; set; }
    }
}