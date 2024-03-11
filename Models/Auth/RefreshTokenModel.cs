
namespace VinhUni_Educator_API.Models
{
    public class RefreshTokenModel
    {
        public string Token { get; set; } = null!;
        public string JwtId { get; set; } = null!;
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime DateExpire { get; set; }
        public string UserId { get; set; } = null!;
    }
}