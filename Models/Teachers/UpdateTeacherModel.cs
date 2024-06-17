namespace VinhUni_Educator_API.Models
{
    public class UpdateTeacherModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Email { get; set; }
        public int? OrganizationId { get; set; }
    }
}