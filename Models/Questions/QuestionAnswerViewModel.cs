namespace VinhUni_Educator_API.Models
{
    public class QuestionAnswerViewModel
    {
        public string? Id { get; set; }
        public string? QuestionId { get; set; }
        public string? AnswerContent { get; set; }
        public string? AnswerImage { get; set; }
        public bool? IsCorrect { get; set; }
    }
}