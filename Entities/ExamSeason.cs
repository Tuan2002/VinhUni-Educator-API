using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RandomString4Net;

namespace VinhUni_Educator_API.Entities
{
    public class ExamSeason
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SeasonCode { get; set; } = RandomString.GetString(Types.ALPHANUMERIC_UPPERCASE, 16);
        public string SeasonName { get; set; } = null!;
        public string? Description { get; set; }
        public string Password { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationInMinutes { get; set; }
        public bool UsePassword { get; set; } = false;
        public bool AllowRetry { get; set; } = false;
        public int MaxRetryTurn { get; set; } = 1;
        public bool IsFinished { get; set; } = false;
        public bool ShowResult { get; set; } = true;
        public bool ShowPoint { get; set; } = true;
        [ForeignKey("Semester")]
        public int SemesterId { get; set; }
        public virtual Semester Semester { get; set; } = null!;
        [ForeignKey("Exam")]
        public string ExamId { get; set; } = null!;
        public virtual Exam Exam { get; set; } = null!;
        [ForeignKey("User")]
        public string CreatedById { get; set; } = null!;
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        [ForeignKey("Teacher")]
        public int OwnerId { get; set; }
        public virtual Teacher Owner { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public virtual ICollection<ExamParticipant> ExamParticipants { get; set; } = null!;
        public virtual ICollection<ExamAssignedClass> AssignedClasses { get; set; } = null!;
    }
}