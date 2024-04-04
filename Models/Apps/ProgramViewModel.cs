namespace VinhUni_Educator_API.Models
{
    public class ProgramViewModel
    {
        public int? Id { get; set; }
        public string? ProgramCode { get; set; }
        public string? ProgramName { get; set; }
        public int? StartYear { get; set; }
        public int? CreditHours { get; set; }
        public float? TrainingYears { get; set; }
        public float? MaxTrainingYears { get; set; }
        public int? MajorId { get; set; }
        public string? MajorName { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}