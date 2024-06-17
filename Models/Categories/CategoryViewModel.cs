namespace VinhUni_Educator_API.Models
{
    public class CategoryViewModel
    {
        public string? Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string? CreatedById { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public bool? IsSharing { get; set; }
        public bool? IsShared { get; set; }
        public DateTime? SharedAt { get; set; }
        public DateOnly? ShareUntil { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}