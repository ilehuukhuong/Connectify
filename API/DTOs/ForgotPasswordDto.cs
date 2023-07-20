using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Please enter an email address")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    }
}