using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Module
    {
        [Key]
        public int Id { get; set; }
        // ModuleId is the same as Id in the USmart School System
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; } = null!;
        public string ModuleName { get; set; } = null!;
        public int CreditHours { get; set; }
        [ForeignKey("SchoolYear")]
        public int? ApplyYearId { get; set; }
        public virtual SchoolYear? ApplyYear { get; set; } = null!;
        [ForeignKey("User")]
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}