namespace VinhUni_Educator_API.Models
{
    public class ExamResultViewModel
    {
        public string? Id { get; set; }
        public string? ExamSeasonCode { get; set; }
        public string? ExamSeasonName { get; set; }
        public string? StudentCode { get; set; }
        public string? StudentName { get; set; }
        public string? TurnId { get; set; }
        public int? TurnNumber { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? TotalQuestions { get; set; }
        public decimal? TotalPoint { get; set; }
        public List<StudentQuestionResult>? ResultQuestions { get; set; }
    }
}