using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class QuestionAnswer
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("Question")]
        public string QuestionId { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
        public string AnswerContent { get; set; } = null!;
        public string? AnswerImage { get; set; }
        public bool IsCorrect { get; set; } = false;
    }
}