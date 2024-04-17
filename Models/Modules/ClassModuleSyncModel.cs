
namespace VinhUni_Educator_API.Models
{
    public class ClassModuleSyncModel
    {
        public int id { get; set; }
        public int idHocKy { get; set; }
        public int idHocPhan { get; set; }
        public string ten { get; set; } = null!;
        public int soSvDangKyToiDa { get; set; }
        public int? idLopHocPhanGoc { get; set; }
        public string code { get; set; } = null!;
    }
}