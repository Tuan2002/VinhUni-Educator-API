namespace VinhUni_Educator_API.Models
{
    public class StudentExamSessionModel
    {
        public string? ExamSeasonCode { get; set; }
        public string? ExamSeasonName { get; set; }
        public string? StudentCode { get; set; }
        public string? StudentName { get; set; }
        public string? TurnId { get; set; }
        public int? TurnNumber { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}