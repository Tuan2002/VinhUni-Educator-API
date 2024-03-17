
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class SyncAction
    {
        [Key]
        public int Id { get; set; }
        public string ActionName { get; set; } = null!;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public DateTime SyncAt { get; set; }
        [ForeignKey("User")]
        public string CreatedBy { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}