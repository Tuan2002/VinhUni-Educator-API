namespace VinhUni_Educator_API.Models
{
    public class QuestionKitViewModel
    {
        public string? Id { get; set; }
        public string? KitName { get; set; }
        public string? KitDescription { get; set; }
        public string? Tag { get; set; }
        public string? CreatedById { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ModifiedById { get; set; }
        public string? ModifiedByName { get; set; }
        public bool? IsShared { get; set; }
        public bool? IsDeleted { get; set; }
        public int? TotalQuestions { get; set; }
    }
}