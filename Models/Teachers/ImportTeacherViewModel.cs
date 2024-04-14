namespace VinhUni_Educator_API.Models
{
    public class ImportTeacherViewModel
    {
        public int? TeacherId { get; set; }
        public int? TeacherCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Gender { get; set; }
        public string? OrganizationCode { get; set; }
        public string? OrganizationName { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Email { get; set; }
        public string? SSOId { get; set; }
        public bool? IsImported { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
}