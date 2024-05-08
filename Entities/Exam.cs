using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Exam
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ExamName { get; set; } = null!;
        public string? ExamDescription { get; set; }
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [ForeignKey("User")]
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        [ForeignKey("Teacher")]
        public int OwnerId { get; set; }
        public virtual Teacher Owner { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = null!;
    }
}