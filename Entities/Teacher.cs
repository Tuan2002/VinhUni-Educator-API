using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }
        // TeacherId is the same as Id in the USmart School System
        public int TeacherId { get; set; }
        public int TeacherCode { get; set; }
        public string? FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int? Gender { get; set; }
        public string? Email { get; set; } = null!;
        public DateOnly? Dob { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; } = null!;
        [ForeignKey("Organization")]
        public int? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; } = null!;
        [ForeignKey("CreatedBy")]
        public string? CreatedById { get; set; } = null!;
        public virtual ApplicationUser? CreatedBy { get; set; } = null!;
        public int? SmartId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsSynced { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}