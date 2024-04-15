using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Semester
    {
        public int Id { get; set; }
        // SemesterId is the same as Id in the USmart School System
        public int SemesterId { get; set; }
        public int SemesterType { get; set; }
        public string SemesterName { get; set; } = null!;
        public string? SemesterShortName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        [ForeignKey("SchoolYear")]
        public int SchoolYearId { get; set; }
        public virtual SchoolYear SchoolYear { get; set; } = null!;
        public string? CreatedById { get; set; } = null!;
        public virtual ApplicationUser? CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}