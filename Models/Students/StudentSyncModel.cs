namespace VinhUni_Educator_API.Models
{
    public class StudentSyncModel
    {
        public int id { get; set; }
        public string ho { get; set; } = null!;
        public string ten { get; set; } = null!;
        public DateTime ngaySinh { get; set; }
        public string? gioiTinh { get; set; }
        public int idLopHanhChinh { get; set; }
        public string code { get; set; } = null!;
        public string idKhoaHoc { get; set; } = null!;
        public string idNganh { get; set; } = null!;
        public string userId { get; set; } = null!;
    }
}