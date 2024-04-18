namespace VinhUni_Educator_API.Models
{
    public class ClassModuleViewModel
    {
        public string? Id { get; set; }
        public string? ModuleClassCode { get; set; }
        public string? ModuleClassName { get; set; }
        public int? ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int? SemesterId { get; set; }
        public bool? IsChildClass { get; set; }
        public string? ParentClassId { get; set; }
        public int? MaxStudents { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}