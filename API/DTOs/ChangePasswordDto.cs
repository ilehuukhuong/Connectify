using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Please enter current password")]
        [RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\w\d\s:])([^\s]){8,}$", ErrorMessage = "Invalid password format")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Please enter new password")]
        [RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\w\d\s:])([^\s]){8,}$", ErrorMessage = "Invalid password format")]
        public string NewPassword { get; set; }
    }
}