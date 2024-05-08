using System.ComponentModel;
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
    [SwaggerTag("Quản lý đề thi của giáo viên")]
    public class ExamsController : ControllerBase
    {
        private readonly IExamManagerServices _examManagerServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public ExamsController(IExamManagerServices examManagerServices)
        {
            _examManagerServices = examManagerServices;
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
    }
}