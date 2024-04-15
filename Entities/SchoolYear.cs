using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Entities
{
    public class SchoolYear
    {
        [Key]
        public int Id { get; set; }
        // SchoolYearId is the same as Id in USmart School System
        public int SchoolYearId { get; set; }
        public int YearCode { get; set; }
        public string SchoolYearName { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? CreatedById { get; set; } = null!;
        public virtual ApplicationUser? CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public virtual ICollection<Semester> Semesters { get; set; } = null!;
    }
}