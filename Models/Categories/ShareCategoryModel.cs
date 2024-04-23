namespace VinhUni_Educator_API.Models
{
    public class ShareCategoryModel
    {
        public List<int> TeacherIds { get; set; } = null!;
        public DateOnly? ShareUntil { get; set; }
    }
}