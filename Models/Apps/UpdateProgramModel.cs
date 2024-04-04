using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace VinhUni_Educator_API.Models
{
    public class UpdateProgramModel
    {
        [SwaggerSchema("Mã chương trình đào tạo")]
        [Required]
        public string? ProgramCode { get; set; }
        [SwaggerSchema("Tên chương trình đào tạo")]
        [Required]
        [MaxLength(255)]
        public string? ProgramName { get; set; }
        [SwaggerSchema("Năm bắt đầu")]
        [Required]
        [Range(1900, 2100)]
        public int? StartYear { get; set; }
        [SwaggerSchema("Số tín chỉ")]
        [Required]
        [Range(1, 200)]
        public int? CreditHours { get; set; }
        [SwaggerSchema("Số năm đào tạo")]
        [Required]
        [Range(1, 10)]
        public float? TrainingYears { get; set; }
        [SwaggerSchema("Số năm đào tạo tối đa")]
        [Required]
        [Range(1, 10)]
        public float? MaxTrainingYears { get; set; }
        [SwaggerSchema("Mã ngành")]
        [Required]
        public int? MajorId { get; set; }
        [SwaggerSchema("Mã khóa học")]
        [Required]
        public int? CourseId { get; set; }
    }
}