namespace VinhUni_Educator_API.Models
{
    public class StudentResult
    {
        public string? StudentCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public decimal? HighestPoint { get; set; }
        public decimal? AveragePoint { get; set; }
        public int? TotalTurns { get; set; }
    }
    public class ExamSeasonReportModel
    {
        public string? ExamSeasonName { get; set; }
        public string? ModuleClassCode { get; set; }
        public string? ModuleClassName { get; set; }
        public string? TeacherName { get; set; }
        public int? TotalStudents { get; set; }
        public DateTime? ExportedAt { get; set; }
        public List<StudentResult>? Records { get; set; }
    }
}