using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace VinhUni_Educator_API.Models
{
    public class ResetPasswordModel
    {
        [Required]
        [SwaggerSchema("Mật khẩu mới")]
        public string NewPassword { get; set; } = null!;
        [Required]
        [SwaggerSchema("Xác nhận mật khẩu mới")]
        public string ConfirmNewPassword { get; set; } = null!;
        public bool CheckSamePassword()
        {
            return NewPassword == ConfirmNewPassword;
        }
    }
}