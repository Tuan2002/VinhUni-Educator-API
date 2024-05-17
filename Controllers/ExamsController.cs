using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Quản lý thi của giáo viên")]
    public class ExamsController : ControllerBase
    {
        private readonly IExamManagerServices _examManagerServices;
        private readonly IExamSeasonServices _examSeasonServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public ExamsController(IExamManagerServices examManagerServices, IExamSeasonServices examSeasonServices)
        {
            _examManagerServices = examManagerServices;
            _examSeasonServices = examSeasonServices;
        }
        [HttpGet]
        [Route("get-exams")]
        [SwaggerOperation(Summary = "Lấy danh sách đề thi", Description = "Lấy danh sách đề thi")]
        public async Task<IActionResult> GetExamsAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _examManagerServices.GetExamsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("create-exam")]
        [SwaggerOperation(Summary = "Tạo đề thi mới", Description = "Tạo đề thi")]
        public async Task<IActionResult> CreateExamAsync([FromBody] CreateExamModel model)
        {
            var response = await _examManagerServices.CreateExamAsync(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-exam/{examId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin đề thi", Description = "Cập nhật thông tin đề thi")]
        public async Task<IActionResult> UpdateExamAsync(string examId, [FromBody] UpdateExamModel model)
        {
            var response = await _examManagerServices.UpdateExamAsync(examId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-exam/{examId}")]
        [SwaggerOperation(Summary = "Xóa đề thi", Description = "Xóa đề thi")]
        public async Task<IActionResult> DeleteExamAsync(string examId)
        {
            var response = await _examManagerServices.DeleteExamAsync(examId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-questions/{examId}")]
        [SwaggerOperation(Summary = "Lấy danh sách câu hỏi của đề thi", Description = "Lấy danh sách câu hỏi của đề thi")]
        public async Task<IActionResult> GetQuestionsByExamAsync(string examId)
        {
            var response = await _examManagerServices.GetQuestionsByExamAsync(examId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("add-questions/{examId}")]
        [SwaggerOperation(Summary = "Thêm câu hỏi vào đề thi", Description = "Thêm câu hỏi vào đề thi")]
        public async Task<IActionResult> AddQuestionsToExamAsync(string examId, [FromBody][SwaggerRequestBody("Danh sách Id của câu hỏi", Required = true)] List<string> questionIds)
        {
            var response = await _examManagerServices.AddQuestionsToExamAsync(examId, questionIds);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("remove-questions/{examId}")]
        [SwaggerOperation(Summary = "Xóa câu hỏi khỏi đề thi", Description = "Xóa câu hỏi khỏi đề thi")]
        public async Task<IActionResult> RemoveQuestionsFromExamAsync(string examId, [FromBody][SwaggerRequestBody("Danh sách Id của câu hỏi", Required = true)] List<string> questionIds)
        {
            var response = await _examManagerServices.RemoveQuestionsFromExamAsync(examId, questionIds);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("create-season")]
        [SwaggerOperation(Summary = "Tạo kỳ thi mới", Description = "Tạo kỳ thi")]
        public async Task<IActionResult> CreateSeasonAsync([FromBody] CreateSeasonModel model)
        {
            var response = await _examSeasonServices.CreateExamSeasonAsync(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-season/{examSeasonId}")]
        [SwaggerOperation(Summary = "Lấy thông tin kỳ thi", Description = "Lấy thông tin kỳ thi")]
        public async Task<IActionResult> GetSeasonAsync(string examSeasonId)
        {
            var response = await _examSeasonServices.GetExamSeasonByIdAsync(examSeasonId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-assigned-classes/{examSeasonId}")]
        [SwaggerOperation(Summary = "Lấy danh sách lớp tham gia thi", Description = "Lấy danh sách lớp tham gia thi")]
        public async Task<IActionResult> GetAssignedClassesAsync(string examSeasonId)
        {
            var response = await _examSeasonServices.GetAssignClassAsync(examSeasonId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-participants/{examSeasonId}")]
        [SwaggerOperation(Summary = "Lấy danh sách sinh viên đã tham gia thi", Description = "Lấy danh sách sinh viên đã tham gia thi")]
        public async Task<IActionResult> GetParticipantsBySeasonAsync(string examSeasonId, string moduleClassId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _examSeasonServices.GetParticipantsBySeasonAsync(examSeasonId, moduleClassId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("assign-classes/{examSeasonId}")]
        [SwaggerOperation(Summary = "Thêm lớp vào kỳ thi", Description = "Thêm lớp vào kỳ thi")]
        public async Task<IActionResult> AddClassesToSeasonAsync(string examSeasonId, [FromBody][SwaggerRequestBody("Danh sách mã lớp học phần", Required = true)] List<string> moduleClassIds)
        {
            var response = await _examSeasonServices.AddClassToExamSeasonAsync(examSeasonId, moduleClassIds);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("remove-classes/{examSeasonId}")]
        [SwaggerOperation(Summary = "Xóa lớp khỏi kỳ thi", Description = "Xóa lớp khỏi kỳ thi")]
        public async Task<IActionResult> RemoveClassesFromSeasonAsync(string examSeasonId, [FromBody][SwaggerRequestBody("Danh sách mã lớp học phần", Required = true)] List<string> moduleClassIds)
        {
            var response = await _examSeasonServices.RemoveClassFromExamSeasonAsync(examSeasonId, moduleClassIds);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("change-exam/{examSeasonId}")]
        [SwaggerOperation(Summary = "Thay đổi đề thi", Description = "Thay đổi đề thi")]
        public async Task<IActionResult> ChangeExamAsync(string examSeasonId, string examId)
        {
            var response = await _examSeasonServices.ChangeExamAsync(examSeasonId, examId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("force-finish/{examSeasonId}")]
        [SwaggerOperation(Summary = "Kết thúc kỳ thi", Description = "Kết thúc kỳ thi")]
        public async Task<IActionResult> ForceFinishSeasonAsync(string examSeasonId)
        {
            var response = await _examSeasonServices.ForceFinishExamSeasonAsync(examSeasonId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-season/{examSeasonId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin kỳ thi", Description = "Cập nhật thông tin kỳ thi")]
        public async Task<IActionResult> UpdateSeasonAsync(string examSeasonId, [FromBody] UpdateSeasonModel model)
        {
            var response = await _examSeasonServices.UpdateExamSeasonAsync(examSeasonId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-season/{examSeasonId}")]
        [SwaggerOperation(Summary = "Xóa kỳ thi", Description = "Xóa kỳ thi")]
        public async Task<IActionResult> DeleteSeasonAsync(string examSeasonId, bool foreverDelete = false)
        {
            var response = await _examSeasonServices.DeleteExamSeasonAsync(examSeasonId, foreverDelete);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-student-turns/{examSeasonId}")]
        [SwaggerOperation(Summary = "Lấy danh sách lượt thi của sinh viên", Description = "Lấy danh sách lượt thi của sinh viên")]
        public async Task<IActionResult> GetStudentExamTurnsAsync(string examSeasonId, int studentId)
        {
            var response = await _examSeasonServices.GetStudentExamTurnsAsync(examSeasonId, studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-exam-result/{turnId}")]
        [SwaggerOperation(Summary = "Lấy kết quả lần thi của sinh viên", Description = "Lấy kết quả lần thi của sinh viên")]
        public async Task<IActionResult> GetStudentExamResultAsync(string turnId)
        {
            var response = await _examSeasonServices.GetStudentExamResultAsync(turnId);
            return StatusCode(response.StatusCode, response);
        }

    }
}