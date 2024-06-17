using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class USmartToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public DateTime ExpireDate { get; set; }
        public bool IsExpired { get; set; }
    }
}