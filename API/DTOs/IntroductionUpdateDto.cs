using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class IntroductionUpdateDto
    {
        [Required(ErrorMessage = "Please enter an introduction")]
        public string Introduction { get; set; }
    }
}