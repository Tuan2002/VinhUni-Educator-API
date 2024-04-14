namespace VinhUni_Educator_API.Models
{
    public class TeacherViewModel
    {
        public int? Id { get; set; }
        public int? TeacherCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Gender { get; set; }
        public string? Email { get; set; }
        public DateOnly? Dob { get; set; }
        public string? UserId { get; set; }
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public string? CreatedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsSynced { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}