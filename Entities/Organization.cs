
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationCode { get; set; } = null!;
        public string? OrganizationName { get; set; }
        [ForeignKey("User")]
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}