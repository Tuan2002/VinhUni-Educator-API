
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class TrainingProgram
    {
        [Key]
        public int Id { get; set; }
        // ProgramId is the same as Id in the USmart School System
        public int ProgramId { get; set; }
        public string ProgramCode { get; set; } = null!;
        public string ProgramName { get; set; } = null!;
        public int? StartYear { get; set; }
        public int CreditHours { get; set; }
        public float? TrainingYears { get; set; }
        public float? MaxTrainingYears { get; set; }
        [ForeignKey("Major")]
        public int MajorId { get; set; }
        public virtual Major Major { get; set; } = null!;
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
        [ForeignKey("User")]
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}