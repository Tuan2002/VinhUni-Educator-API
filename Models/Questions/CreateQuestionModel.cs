namespace VinhUni_Educator_API.Models
{
    public class CreateQuestionModel
    {
        public string QuestionContent { get; set; } = null!;
        public string? QuestionNote { get; set; }
        public List<string>? QuestionImages { get; set; }
        public bool IsMultipleChoice { get; set; } = false;
        public int Level { get; set; }
        public List<QuestionAnswerModel> Answers { get; set; } = null!;

    }
    public class QuestionAnswerModel
    {
        public string AnswerContent { get; set; } = null!;
        public string? AnswerImage { get; set; }
        public bool IsCorrect { get; set; }
    }
}