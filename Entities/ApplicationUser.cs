
using Microsoft.AspNetCore.Identity;

namespace VinhUni_Educator_API.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? USmartId { get; set; }
        public string? FirstName { get; set; }
        public string LastName { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPasswordChanged { get; set; } = false;
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public virtual ICollection<IdentityUserRole<string>>? UserRoles { get; set; }
        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}