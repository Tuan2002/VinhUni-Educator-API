namespace VinhUni_Educator_API.Models
{
    public class ExamViewModel
    {
        public string? Id { get; set; }
        public string? ExamName { get; set; }
        public string? ExamDescription { get; set; }
        public bool? IsPublished { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedById { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public bool? IsDeleted { get; set; }
        public int? TotalQuestions { get; set; }
        public int? EasyQuestions { get; set; }
        public int? MediumQuestions { get; set; }
        public int? HardQuestions { get; set; }
    }
}