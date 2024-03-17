
namespace VinhUni_Educator_API.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int StartYear { get; set; }
        public string CreatedBy { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public string? DeletedBy { get; set; }
    }
}