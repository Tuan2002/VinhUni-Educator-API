namespace VinhUni_Educator_API.Models
{
    public class ModuleSyncModel
    {
        public int id { get; set; }
        public string code { get; set; } = null!;
        public string ten { get; set; } = null!;
        public int soTinChi { get; set; }
        public int? namApDung { get; set; }
    }
}