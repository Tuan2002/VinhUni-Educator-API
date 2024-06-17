using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ExamQuestion
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("Exam")]
        public string ExamId { get; set; } = null!;
        public virtual Exam Exam { get; set; } = null!;
        [ForeignKey("QuestionKit")]
        public string QuestionKitId { get; set; } = null!;
        public virtual QuestionKit QuestionKit { get; set; } = null!;
        [ForeignKey("Question")]
        public string QuestionId { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
        public DateTime AddedAt { get; set; }
    }
}