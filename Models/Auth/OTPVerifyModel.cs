
namespace VinhUni_Educator_API.Models
{
    public class OTPVerifyModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = null!;
        public string OTP { get; set; } = null!;
    }
}