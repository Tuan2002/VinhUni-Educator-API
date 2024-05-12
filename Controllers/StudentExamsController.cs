using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/student-exams")]
    [SwaggerTag("Bài thi của sinh viên")]
    public class StudentExamsController : ControllerBase
    {
        private readonly IStudentExamServices _studentExamServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 20;
        public StudentExamsController(IStudentExamServices studentExamServices)
        {
            _studentExamServices = studentExamServices;
        }
        [HttpGet]
        [Route("get-exam-season/{examSeasonCode}")]
        [SwaggerOperation(Summary = "Lấy thông tin kỳ thi", Description = "Lấy thông tin kỳ thi theo mã kỳ thi")]
        public async Task<IActionResult> GetExamSeasonAsync(string examSeasonCode)
        {
            var response = await _studentExamServices.GetExamSeasonAsync(examSeasonCode);
            return Ok(response);
        }
        [HttpGet]
        [Route("get-exam-turns/{examSeasonCode}")]
        [SwaggerOperation(Summary = "Lấy thông tin các lượt thi", Description = "Lấy thông tin các lượt thi")]
        public async Task<IActionResult> GetExamTurnsAsync(string examSeasonCode)
        {
            var response = await _studentExamServices.GetExamTurnsAsync(examSeasonCode);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("start-exam/{examSeasonCode}")]
        [SwaggerOperation(Summary = "Bắt đầu thi", Description = "Bắt đầu thi")]
        public async Task<IActionResult> StartExamAsync(string examSeasonCode, string moduleClassId)
        {
            var response = await _studentExamServices.StartExamAsync(examSeasonCode, moduleClassId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("force-finish-turn/{examSeasonCode}/{turnId}")]
        [SwaggerOperation(Summary = "Buộc kết thúc lượt thi", Description = "Buộc kết thúc lượt thi")]
        public async Task<IActionResult> ForceFinishExamTurnAsync(string examSeasonCode, string turnId)
        {
            var response = await _studentExamServices.ForceFinishExamTurnAsync(examSeasonCode, turnId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-exam-questions/{seasonCode}")]
        [SwaggerOperation(Summary = "Lấy danh sách câu hỏi thi", Description = "Lấy danh sách câu hỏi thi")]
        public async Task<IActionResult> GetExamQuestionsAsync(string seasonCode, int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _studentExamServices.GetExamQuestionsAsync(seasonCode, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("submit-exam/{seasonCode}/{turnId}")]
        [SwaggerOperation(Summary = "Nộp bài thi", Description = "Nộp bài thi")]
        public async Task<IActionResult> SubmitExamAnswersAsync(string seasonCode, string turnId, [FromBody] List<SubmitQuestionModel> submitQuestions)
        {
            var response = await _studentExamServices.SubmitExamAnswersAsync(seasonCode, turnId, submitQuestions);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("resume-exam-turn/{seasonCode}/{turnId}")]
        [SwaggerOperation(Summary = "Tiếp tục lượt thi", Description = "Lượt thi")]
        public async Task<IActionResult> ResumeExamTurnAsync(string seasonCode, string turnId)
        {
            var response = await _studentExamServices.ResumeExamTurnAsync(seasonCode, turnId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-exam-result/{seasonCode}/{turnId}")]
        [SwaggerOperation(Summary = "Lấy kết quả thi", Description = "Lấy kết quả thi")]
        public async Task<IActionResult> GetExamResultAsync(string seasonCode, string turnId)
        {
            var response = await _studentExamServices.GetExamResultAsync(seasonCode, turnId);
            return StatusCode(response.StatusCode, response);
        }
    }
}