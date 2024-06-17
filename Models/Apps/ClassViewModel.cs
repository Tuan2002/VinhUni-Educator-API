namespace VinhUni_Educator_API.Models
{
    public class ClassViewModel
    {
        public int? Id { get; set; }
        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }
        public int? ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}