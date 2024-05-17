namespace VinhUni_Educator_API.Models
{
    public class ExamParticipantViewModel
    {
        public string? Id { get; set; }
        public string? ExamSeasonId { get; set; }
        public int? StudentId { get; set; }
        public string? StudentCode { get; set; }
        public string? StudentName { get; set; }
        public DateTime? JoinedAt { get; set; }
        public int? TotalTurn { get; set; }
        public int? TotalFinishedTurn { get; set; }
        public bool? IsAllTurnFinished { get; set; }
        public DateTime? LastTurnFinishedAt { get; set; }
        public decimal? HighestPoint { get; set; }
        public decimal? AveragePoint { get; set; }
    }
}