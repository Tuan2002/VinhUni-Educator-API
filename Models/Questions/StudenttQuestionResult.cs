namespace VinhUni_Educator_API.Models
{
    public class StudentQuestionResult
    {
        public string? Id { get; set; }
        public string? QuestionContent { get; set; }
        public string? QuestionNote { get; set; }
        public List<string>? QuestionImages { get; set; }
        public bool? IsMultipleChoice { get; set; }
        public int? Order { get; set; }
        public List<QuestionAnswerResult>? Answers { get; set; }
    }
}