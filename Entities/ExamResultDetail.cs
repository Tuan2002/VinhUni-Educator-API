using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ExamResultDetail
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("ExamResult")]
        public string ExamResultId { get; set; } = null!;
        public virtual ExamResult ExamResult { get; set; } = null!;
        [ForeignKey("Question")]
        public string QuestionId { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
        [ForeignKey("QuestionAnswer")]
        public string? SelectedAnswerId { get; set; }
        public virtual QuestionAnswer? SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}