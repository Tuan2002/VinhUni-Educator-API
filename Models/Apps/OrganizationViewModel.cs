
namespace VinhUni_Educator_API.Models
{
    public class OrganizationViewModel
    {
        public int Id { get; set; }
        public string? OrganizationCode { get; set; }
        public string? OrganizationName { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}