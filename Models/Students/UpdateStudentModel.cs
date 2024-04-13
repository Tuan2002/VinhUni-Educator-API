namespace VinhUni_Educator_API.Models
{
    public class UpdateStudentModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public int? ClassId { get; set; }
        public int? ProgramId { get; set; }
        public int? CourseId { get; set; }
    }
}