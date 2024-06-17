namespace VinhUni_Educator_API.Entities
{
    public class SchoolYearViewModel
    {
        public int? Id { get; set; }
        public int? YearCode { get; set; }
        public string? SchoolYearName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CreatedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}