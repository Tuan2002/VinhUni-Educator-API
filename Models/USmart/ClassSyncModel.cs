
namespace VinhUni_Educator_API.Models
{
    public class ClassSyncModel
    {
        public int id { get; set; }
        public string code { get; set; } = null!;
        public string ten { get; set; } = null!;
        public string idNganh { get; set; } = null!;
        public string idKhoaHoc { get; set; } = null!;

    }
}