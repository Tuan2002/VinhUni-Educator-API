
namespace VinhUni_Educator_API.Models
{
    public class PublicUserModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsPasswordChanged { get; set; }
        public IList<string>? Roles { get; set; }
    }
}