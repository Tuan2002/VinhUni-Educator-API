using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ModuleClassStudent
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("ClassModule")]
        public string ModuleClassId { get; set; } = null!;
        public virtual ModuleClass ModuleClass { get; set; } = null!;
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;
        [ForeignKey("Semester")]
        public int SemesterId { get; set; }
        public virtual Semester Semester { get; set; } = null!;
        public DateTime AddedAt { get; set; }
        [ForeignKey("User")]
        public string AddedById { get; set; } = null!;
        public virtual ApplicationUser AddedBy { get; set; } = null!;

    }
}