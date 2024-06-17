namespace VinhUni_Educator_API.Models
{
    public class SemesterSyncModel
    {
        public int id { get; set; }
        public int namHoc { get; set; }
        public int type { get; set; }
        public string ten { get; set; } = null!;
        public string? tenRutGon { get; set; }
        public DateTime tuNgay { get; set; }
        public DateTime denNgay { get; set; }
    }
}