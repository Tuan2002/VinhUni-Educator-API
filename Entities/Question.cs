using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Question
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("QuestionKit")]
        public string QuestionKitId { get; set; } = null!;
        public virtual QuestionKit QuestionKit { get; set; } = null!;
        public string QuestionContent { get; set; } = null!;
        public string? QuestionNote { get; set; }
        public List<string>? QuestionImages { get; set; }
        public bool IsMultipleChoice { get; set; } = false;
        public int Level { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<QuestionAnswer> Answers { get; set; } = null!;
    }
}