using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Please enter an email address")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please enter a token")]
        public string Token { get; set; }
        [Required(ErrorMessage = "Please enter a password")]
        [RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\w\d\s:])([^\s]){8,}$", ErrorMessage = "Invalid password format")]
        public string NewPassword { get; set; }
    }
}