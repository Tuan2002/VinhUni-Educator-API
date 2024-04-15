using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        // CourseId is the same as Id in the USmart School System
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int StartYear { get; set; }
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}