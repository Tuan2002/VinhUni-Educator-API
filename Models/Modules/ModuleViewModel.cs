namespace VinhUni_Educator_API.Models
{
    public class ModuleViewModel
    {
        public int? Id { get; set; }
        public string? ModuleCode { get; set; }
        public string? ModuleName { get; set; }
        public int? CreditHours { get; set; }
        public int? ApplyYearId { get; set; }
        public string? ApplyYearName { get; set; }
        public string? CreatedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}