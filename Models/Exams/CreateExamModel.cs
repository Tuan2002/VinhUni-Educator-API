using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Models
{
    public class CreateExamModel
    {
        [Required]
        public string ExamName { get; set; } = null!;
        public string? ExamDescription { get; set; }
        public bool IsPublished { get; set; } = true;
        public List<string>? QuestionIds { get; set; }
    }
}