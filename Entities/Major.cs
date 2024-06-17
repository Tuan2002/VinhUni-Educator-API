
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Major
    {
        [Key]
        public int Id { get; set; }
        // MajorId is the same as Id in the USmart School System
        public int MajorId { get; set; }
        public string? MajorCode { get; set; } = null!;
        public string MajorName { get; set; } = null!;
        public float? MinTrainingYears { get; set; }
        public float? MaxTrainingYears { get; set; }
        [ForeignKey("User")]
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

    }
}