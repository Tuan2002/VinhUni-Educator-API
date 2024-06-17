namespace VinhUni_Educator_API.Models
{
    public class ImportStudentViewModel
    {
        public int? StudentId { get; set; }
        public string? StudentCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? Dob { get; set; }
        public int? Gender { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? ProgramCode { get; set; }
        public string? ProgramName { get; set; }
        public bool IsImported { get; set; } = false;
        public string? ErrorMessage { get; set; }

    }
}