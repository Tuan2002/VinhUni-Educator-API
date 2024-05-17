namespace VinhUni_Educator_API.Models
{
    public class StudentExamTurnModel
    {
        public string? Id { get; set; }
        public string? ExamSeasonCode { get; set; }
        public string? ExamSeasonName { get; set; }
        public int? StudentId { get; set; }
        public string? StudentCode { get; set; }
        public string? StudentName { get; set; }
        public int? TurnNumber { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool? IsFinished { get; set; }
        public bool? AllowContinue { get; set; }
        public bool? AllowViewResult { get; set; }
        public decimal? TotalPoint { get; set; }
        public string? ExamResultId { get; set; }
    }
}