namespace VinhUni_Educator_API.Models
{
    public class MajorViewModel
    {
        public int? Id { get; set; }
        public string? MajorCode { get; set; }
        public string? MajorName { get; set; }
        public float? MinTrainingYears { get; set; }
        public float? MaxTrainingYears { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

    }
}