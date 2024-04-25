namespace VinhUni_Educator_API.Models
{
    public class CreateQuestionResult
    {
        public string? QuestionContent { get; set; }
        public string? QuestionNote { get; set; }
        public List<string>? QuestionImages { get; set; }
        public bool? IsMultipleChoice { get; set; }
        public int? Level { get; set; }
        public List<QuestionAnswerModel>? Answers { get; set; }
        public bool IsImported { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

}