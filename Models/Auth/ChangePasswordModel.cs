
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace VinhUni_Educator_API.Models
{
    public class ChangePasswordModel
    {
        [Required]
        [SwaggerSchema("Mật khẩu cũ")]
        public string OldPassword { get; set; } = null!;
        [Required]
        [MinLength(8)]
        [SwaggerSchema("Mật khẩu mới")]
        public string NewPassword { get; set; } = null!;
        [Required]
        [MinLength(8)]
        [SwaggerSchema("Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; } = null!;
        public bool CheckSamePassword()
        {
            return NewPassword == ConfirmPassword;
        }
        public bool CheckSameOldPassword()
        {
            return OldPassword != NewPassword;
        }
    }
}