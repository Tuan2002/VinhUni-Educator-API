
using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Models
{
    public class CreateUserModel
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string? Email { get; set; }
        public int? USmartId { get; set; }
        public string? FirstName { get; set; }
        [Required]
        public string LastName { get; set; } = null!;
        public int Gender { get; set; }
        [Required]
        public DateOnly DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public List<string>? Roles { get; set; }
        public string GeneratePassword()
        {
            return "VinhUni" + "@" + DateOfBirth.ToString("ddMMyyyy");
        }
    }
}