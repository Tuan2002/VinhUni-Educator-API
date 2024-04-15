namespace VinhUni_Educator_API.Models
{
    public class SchoolYearSyncModel
    {
        public int id { get; set; }
        public int nam { get; set; }
        public string ten { get; set; } = null!;
        public DateTime tuNgay { get; set; }
        public DateTime denNgay { get; set; }
    }
}