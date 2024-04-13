namespace VinhUni_Educator_API.Models
{
    public class StudentViewModel
    {
        public int Id { get; set; }
        public string? StudentCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public int? ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsSynced { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}