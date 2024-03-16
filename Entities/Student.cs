using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; } = null!;
        [ForeignKey("PrimaryClass")]
        public int? ClassId { get; set; }
        public virtual PrimaryClass? PrimaryClass { get; set; } = null!;
        [ForeignKey("TrainingProgram")]
        public int? ProgramId { get; set; }
        public virtual TrainingProgram? TrainingProgram { get; set; } = null!;
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
        [ForeignKey("CreatedBy")]
        public string? CreatedById { get; set; } = null!;
        public virtual ApplicationUser? CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsSynced { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}