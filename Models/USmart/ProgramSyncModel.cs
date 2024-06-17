
namespace VinhUni_Educator_API.Models
{
    public class ProgramSyncModel
    {
        public int id { get; set; }
        public string code { get; set; } = null!;
        public string ten { get; set; } = null!;
        public int idKhoaHoc { get; set; }
        public int idNganh { get; set; }
        public int soTinChi { get; set; }
        public int? namBatDau { get; set; }
        public float? soNamDaoTao { get; set; }
        public float? soNamDaoTaoToiDa { get; set; }
    }
}