using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace VinhUni_Educator_API.Models
{
    public class UpdateSeasonModel
    {
        [SwaggerSchema(Description = "Tên của kỳ thi")]
        public string? SeasonName { get; set; }
        [SwaggerSchema(Description = "Mô tả, ghi chú của kỳ thi")]
        public string? Description { get; set; }
        [SwaggerSchema(Description = "Mật khẩu của kỳ thi, có thể để trống nếu sử dụng mật khẩu ngẫu nhiên")]
        public string? Password { get; set; }
        [SwaggerSchema(Description = "Thời gian bắt đầu của kỳ thi")]
        public DateTime? StartTime { get; set; }
        [SwaggerSchema(Description = "Thời gian kết thúc của kỳ thi")]
        public DateTime? EndTime { get; set; }
        [SwaggerSchema(Description = "Thời gian làm bài của kỳ thi (phút)")]
        [Range(5, 360)]
        public int? DurationInMinutes { get; set; }
        [SwaggerSchema(Description = "Mã học kỳ")]
        public int? SemesterId { get; set; }
        [SwaggerSchema(Description = "Yêu cầu mật khẩu để tham gia thi")]
        public bool? UsePassword { get; set; }
        [SwaggerSchema(Description = "Cho phép làm lại")]
        public bool? AllowRetry { get; set; }
        [SwaggerSchema(Description = "Số lần làm lại tối đa")]
        public int? MaxRetryTurn { get; set; }
        [SwaggerSchema(Description = "Hiển thị kết quả thi")]
        public bool? ShowResult { get; set; }
        [SwaggerSchema(Description = "Hiển thị điểm")]
        public bool? ShowPoint { get; set; }
    }
}