namespace VinhUni_Educator_API.Models
{
    public class ExamTurnViewModel
    {
        public string? Id { get; set; }
        public int? TurnNumber { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool? IsFinished { get; set; }
        public string? ExamResultId { get; set; }
    }
}