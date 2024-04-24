using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class QuestionKit
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string KitName { get; set; } = null!;
        public string? KitDescription { get; set; }
        public string? Tag { get; set; }
        [ForeignKey("Category")]
        public string CategoryId { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        [ForeignKey("User")]
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        [ForeignKey("Teacher")]
        public int OwnerId { get; set; }
        public virtual Teacher Owner { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [ForeignKey("User")]
        public string? ModifiedById { get; set; }
        public virtual ApplicationUser? ModifiedBy { get; set; }
        public bool IsShared { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<Question> Questions { get; set; } = null!;
    }
}