namespace VinhUni_Educator_API.Models
{
    public class QuestionAnswerResult
    {
        public string? Id { get; set; }
        public string? QuestionId { get; set; }
        public string? AnswerContent { get; set; }
        public string? AnswerImage { get; set; }
        public bool? IsCorrect { get; set; }
        public bool? IsSelected { get; set; }
    }
}