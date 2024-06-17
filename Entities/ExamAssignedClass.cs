using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhUni_Educator_API.Entities
{
    public class ExamAssignedClass
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("ExamSeason")]
        public string ExamSeasonId { get; set; } = null!;
        public virtual ExamSeason ExamSeason { get; set; } = null!;
        [ForeignKey("ModuleClass")]
        public string ModuleClassId { get; set; } = null!;
        public virtual ModuleClass ModuleClass { get; set; } = null!;
        public DateTime AddedAt { get; set; }
    }
}