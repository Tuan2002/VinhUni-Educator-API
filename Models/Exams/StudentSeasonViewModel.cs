namespace VinhUni_Educator_API.Models
{
    public class StudentSeasonViewModel
    {
        public string? SeasonCode { get; set; }
        public string? SeasonName { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationInMinutes { get; set; }
        public bool? AllowRetry { get; set; }
        public int? MaxRetryTurn { get; set; }
        public int? RemainingRetryTurn { get; set; }
        public bool? IsFinished { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}