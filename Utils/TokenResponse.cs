
namespace VinhUni_Educator_API.Utils
{
    public class TokenResponse
    {
        public string TokenId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}