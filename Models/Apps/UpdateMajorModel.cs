using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace VinhUni_Educator_API.Models
{
    public class UpdateMajorModel
    {
        [SwaggerSchema("Mã ngành")]
        public string? MajorCode { get; set; }
        [SwaggerSchema("Tên ngành")]
        public string? MajorName { get; set; }
        [SwaggerSchema("Số năm đào tạo tối thiểu")]
        public float? MinTrainingYears { get; set; }
        [SwaggerSchema("Số năm đào tạo tối đa")]
        public float? MaxTrainingYears { get; set; }
    }
}