using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class SharedCategory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("Category")]
        public string CategoryId { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        [ForeignKey("Teacher")]
        public int ViewerId { get; set; }
        public virtual Teacher Viewer { get; set; } = null!;
        [ForeignKey("Teacher")]
        public int SharedById { get; set; }
        public virtual Teacher SharedBy { get; set; } = null!;
        public DateTime SharedAt { get; set; }
        public DateOnly? SharedUntil { get; set; }
    }
}