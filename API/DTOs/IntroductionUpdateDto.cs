using API.Extensions;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class IntroductionUpdateDto
    {
        [Required(ErrorMessage = "Please enter an introduction")]
        [MaxWord(200, ErrorMessage = "The introduction must be less than 200 words.")]
        public string Introduction { get; set; }
    }
}