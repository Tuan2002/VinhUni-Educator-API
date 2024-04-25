namespace VinhUni_Educator_API.Models
{
    public class UpdateQuestionModel
    {
        public string Id { get; set; } = null!;
        public string QuestionContent { get; set; } = null!;
        public string? QuestionNote { get; set; }
        public List<string>? QuestionImages { get; set; }
        public bool IsMultipleChoice { get; set; }
        public int Level { get; set; }
        public int Order { get; set; }
        public List<UpdateAnswerModel> Answers { get; set; } = null!;
    }
    public class UpdateAnswerModel
    {
        public string Id { get; set; } = null!;
        public string AnswerContent { get; set; } = null!;
        public string? AnswerImage { get; set; }
        public bool IsCorrect { get; set; }

    }
}