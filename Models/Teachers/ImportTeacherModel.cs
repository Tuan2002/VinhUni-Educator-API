namespace VinhUni_Educator_API.Models
{
    public class ImportTeacherModel
    {
        public int TeacherId { get; set; }
        public int TeacherCode { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int? Gender { get; set; }
        public string? OrganizationCode { get; set; }
        public DateOnly Dob { get; set; }
        public string? Email { get; set; } = null!;
        public int SSOId { get; set; }
    }
}