namespace VinhUni_Educator_API.Models
{
    public class QuestionViewModel
    {
        public string? Id { get; set; }
        public string? QuestionKitId { get; set; }
        public string? QuestionContent { get; set; }
        public string? QuestionNote { get; set; }
        public List<string>? QuestionImages { get; set; }
        public bool? IsMultipleChoice { get; set; }
        public int? Level { get; set; }
        public int? Order { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<QuestionAnswerViewModel>? Answers { get; set; }
    }
}