using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ExamParticipant
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("ExamSeason")]
        public string ExamSeasonId { get; set; } = null!;
        public virtual ExamSeason ExamSeason { get; set; } = null!;
        [ForeignKey("AssignedClass")]
        public string AssignedClassId { get; set; } = null!;
        public virtual ExamAssignedClass AssignedClass { get; set; } = null!;
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
        public virtual ICollection<ExamTurn> ExamTurns { get; set; } = null!;
    }
}