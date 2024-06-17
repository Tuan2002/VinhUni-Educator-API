namespace VinhUni_Educator_API.Models
{
    public class AssignedClassViewModel
    {
        public string? Id { get; set; }
        public string? ExamSeasonId { get; set; }
        public string? ModuleClassId { get; set; }
        public string? ModuleClassName { get; set; }
        public DateTime AddedAt { get; set; }
    }
}