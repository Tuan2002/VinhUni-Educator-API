
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Major
    {
        [Key]
        public int Id { get; set; }
        public int MajorId { get; set; }
        public string? MajorCode { get; set; } = null!;
        public string MajorName { get; set; } = null!;
        public float? MinTrainingYears { get; set; }
        public float? MaxTrainingYears { get; set; }
        [ForeignKey("User")]
        public string CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

    }
}