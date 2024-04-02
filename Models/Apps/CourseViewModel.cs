
using VinhUni_Educator_API.Entities;

namespace VinhUni_Educator_API.Models
{
    public class CourseViewModel
    {
        public int? Id { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public int? StartYear { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}