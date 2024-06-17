namespace VinhUni_Educator_API.Models
{
    public class SemesterViewModel
    {
        public int? Id { get; set; }
        public int? SemesterType { get; set; }
        public string? SemesterName { get; set; }
        public string? SemesterShortName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? SchoolYearId { get; set; }
        public string? SchoolYearName { get; set; }
        public string? CreatedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}