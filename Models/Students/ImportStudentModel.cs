namespace VinhUni_Educator_API.Models
{
    public class ImportStudentModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly Dob { get; set; }
        public int? Gender { get; set; }
        public int ClassId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string ProgramCode { get; set; } = null!;
        public int SSOId { get; set; }
    }
}