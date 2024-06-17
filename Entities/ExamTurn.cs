using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ExamTurn
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("ExamSeason")]
        public string ExamSeasonId { get; set; } = null!;
        public virtual ExamSeason ExamSeason { get; set; } = null!;
        [ForeignKey("ExamParticipant")]
        public string ExamParticipantId { get; set; } = null!;
        public virtual ExamParticipant ExamParticipant { get; set; } = null!;
        public int TurnNumber { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsFinished { get; set; } = false;
        public virtual ExamResult? ExamResult { get; set; }
    }
}