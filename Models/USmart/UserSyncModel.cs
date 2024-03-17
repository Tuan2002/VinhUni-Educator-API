
namespace VinhUni_Educator_API.Models
{
    public class UserSyncModel
    {
        public int id { get; set; }
        public string userName { get; set; } = null!;
        public string? email { get; set; } = null!;
        public string? phoneNumber { get; set; } = null!;
        public string firstName { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public int? gender { get; set; }
        public DateTime dob { get; set; }
        public string? source { get; set; }
        public string GeneratePassword()
        {
            return "VinhUni" + "@" + dob.ToString("ddMMyyyy");
        }
    }
}