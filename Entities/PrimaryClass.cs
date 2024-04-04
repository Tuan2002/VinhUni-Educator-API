
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class PrimaryClass
    {
        [Key]
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        [ForeignKey("TrainingProgram")]
        public int ProgramId { get; set; }
        public virtual TrainingProgram Program { get; set; } = null!;
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