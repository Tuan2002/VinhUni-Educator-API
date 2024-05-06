namespace VinhUni_Educator_API.Models
{
    public class SharedCategoryViewModel
    {
        public DateTime? SharedAt { get; set; }
        public DateOnly? SharedUntil { get; set; }
        public string? CategoryId { get; set; }
        public int? TeacherCode { get; set; }
        public string? TeacherName { get; set; }
    }
}