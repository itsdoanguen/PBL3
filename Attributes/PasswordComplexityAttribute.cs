using System.ComponentModel.DataAnnotations;

namespace PBL3.Attributes
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Password is required");
            }

            var password = value.ToString();
        
            if (password.Length < 6 || password.Length > 18)
            {
                return new ValidationResult("The length of password must be greater than 6 and smaller than 18");
            }
            if (!password.Any(char.IsDigit))
            {
                return new ValidationResult("Password must contain at least one digit");
            }
            if (!password.Any(char.IsUpper))
            {
                return new ValidationResult("Password must contain at least one uppercase letter");
            }
            if (!password.Any(char.IsLower))
            {
                return new ValidationResult("Password must contain at least one lowercase letter");
            }
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return new ValidationResult("Password must contain at least one special character");
            }
            if (password.Contains(" "))
            {
                return new ValidationResult("Password must not contain any whitespace");
            }

            return ValidationResult.Success;
        }
    }
}
