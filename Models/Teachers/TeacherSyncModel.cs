
namespace VinhUni_Educator_API.Models
{
    public class TeacherSyncModel
    {
        public int id { get; set; }
        public int hS_ID { get; set; }
        public string hS_Ho { get; set; } = null!;
        public string hS_Ten { get; set; } = null!;
        public int? hS_GioiTinh { get; set; }
        public string? dV_ID_GiangDay { get; set; }
        public DateTime ngaySinh { get; set; }
        public string? hS_Email { get; set; }
        public string userId { get; set; } = null!;
    }
}