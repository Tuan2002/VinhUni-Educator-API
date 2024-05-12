using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ExamResult
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("ExamTurn")]
        public string ExamTurnId { get; set; } = null!;
        public virtual ExamTurn ExamTurn { get; set; } = null!;
        public decimal TotalPoint { get; set; } = 0;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<ExamResultDetail> ExamResultDetails { get; set; } = null!;
    }
}