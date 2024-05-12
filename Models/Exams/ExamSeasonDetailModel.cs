namespace VinhUni_Educator_API.Models
{
    public class ExamSeasonDetailModel
    {
        public string? Id { get; set; }
        public string? SeasonCode { get; set; }
        public string? SeasonName { get; set; }
        public string? Description { get; set; }
        public string? Password { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationInMinutes { get; set; }
        public bool? UsePassword { get; set; }
        public bool? AllowRetry { get; set; }
        public int? MaxRetryTurn { get; set; }
        public bool? ShowResult { get; set; }
        public bool? ShowPoint { get; set; }
        public bool? IsFinished { get; set; }
        public string? ExamId { get; set; }
        public string? ExamName { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public int? TotalClasses { get; set; }
    }
}