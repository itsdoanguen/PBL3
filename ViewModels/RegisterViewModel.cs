using System.ComponentModel.DataAnnotations;
using PBL3.Attributes;

namespace PBL3.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required"), DataType(DataType.Password), PasswordComplexity]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required"), DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
