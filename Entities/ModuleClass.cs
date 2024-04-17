using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ModuleClass
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        // ModuleClassId is the same as Id in the USmart School System
        public int ModuleClassId { get; set; }
        public string ModuleClassCode { get; set; } = null!;
        public string ModuleClassName { get; set; } = null!;
        [ForeignKey("Module")]
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;
        [ForeignKey("Semester")]
        public int SemesterId { get; set; }
        public virtual Semester Semester { get; set; } = null!;
        public bool IsChildClass { get; set; } = false;
        [ForeignKey("ParentClass")]
        public string? ParentClassId { get; set; }
        public virtual ModuleClass? ParentClass { get; set; }
        public int MaxStudents { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}