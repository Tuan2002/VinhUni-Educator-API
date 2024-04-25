using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý ngân hàng câu hỏi")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionServices _questionServices;
        private readonly IQuestionManagerServices _questionManagerServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public QuestionsController(IQuestionServices questionServices, IQuestionManagerServices questionManagerServices)
        {
            _questionServices = questionServices;
            _questionManagerServices = questionManagerServices;
        }
        [HttpPost]
        [Route("create-question-kit")]
        [SwaggerOperation(Summary = "Tạo bộ câu hỏi", Description = "Tạo bộ câu hỏi mới")]
        public async Task<IActionResult> CreateQuestionKitAsync([FromBody] CreateQuestionKitModel questionKit)
        {
            var response = await _questionServices.CreateQuestionKitAsync(questionKit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-question-kits/{categoryId}")]
        [SwaggerOperation(Summary = "Lấy danh sách bộ câu hỏi", Description = "Lấy danh sách bộ câu hỏi theo danh mục")]
        public async Task<IActionResult> GetQuestionKitsAsync(string categoryId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _questionServices.GetQuestionKitsAsync(categoryId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-question-kit/{questionKitId}")]
        [SwaggerOperation(Summary = "Lấy thông tin bộ câu hỏi", Description = "Lấy thông tin bộ câu hỏi theo Id")]
        public async Task<IActionResult> GetQuestionKitByIdAsync(string questionKitId)
        {
            var response = await _questionServices.GetQuestionKitByIdAsync(questionKitId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-question-kit/{questionKitId}")]
        [SwaggerOperation(Summary = "Cập nhật bộ câu hỏi", Description = "Cập nhật thông tin bộ câu hỏi")]
        public async Task<IActionResult> UpdateQuestionKitAsync(string questionKitId, [FromBody] UpdateQuestionKitModel questionKit)
        {
            var response = await _questionServices.UpdateQuestionKitAsync(questionKitId, questionKit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-question-kit/{questionKitId}")]
        [SwaggerOperation(Summary = "Xóa bộ câu hỏi", Description = "Xóa bộ câu hỏi theo Id")]
        public async Task<IActionResult> DeleteQuestionKitAsync(string questionKitId)
        {
            var response = await _questionServices.DeleteQuestionKitAsync(questionKitId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("share-question-kit/{questionKitId}")]
        [SwaggerOperation(Summary = "Chia sẻ bộ câu hỏi", Description = "Chia sẻ bộ câu hỏi")]
        public async Task<IActionResult> ShareQuestionKitAsync(string questionKitId)
        {
            var response = await _questionServices.ShareQuestionKitAsync(questionKitId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("unshare-question-kit/{questionKitId}")]
        [SwaggerOperation(Summary = "Hủy chia sẻ bộ câu hỏi", Description = "Hủy chia sẻ bộ câu hỏi")]
        public async Task<IActionResult> UnShareQuestionKitAsync(string questionKitId)
        {
            var response = await _questionServices.UnShareQuestionKitAsync(questionKitId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("import-questions/{questionKitId}")]
        [SwaggerOperation(Summary = "Nhập câu hỏi", Description = "Nhập câu hỏi vào bộ câu hỏi")]
        public async Task<IActionResult> ImportQuestionsAsync(string questionKitId, [FromBody] List<CreateQuestionModel> questions)
        {
            var response = await _questionManagerServices.ImportQuestionsAsync(questionKitId, questions);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-questions/{questionKitId}")]
        [SwaggerOperation(Summary = "Lấy danh sách câu hỏi", Description = "Lấy danh sách câu hỏi theo bộ câu hỏi")]
        public async Task<IActionResult> GetQuestionsNByKitAsync(string questionKitId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _questionManagerServices.GetQuestionsNByKitAsync(questionKitId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-question/{questionId}")]
        [SwaggerOperation(Summary = "Lấy thông tin câu hỏi", Description = "Lấy thông tin câu hỏi theo Id")]
        public async Task<IActionResult> GetQuestionByIdAsync(string questionId)
        {
            var response = await _questionManagerServices.GetQuestionByIdAsync(questionId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-questions/{questionKitId}")]
        [SwaggerOperation(Summary = "Cập nhật các câu hỏi", Description = "Cập nhật thông tin câu hỏi")]
        public async Task<IActionResult> UpdateQuestionsByKitAsync(string questionKitId, [FromBody] List<UpdateQuestionModel> questionsToUpdate)
        {
            var response = await _questionManagerServices.UpdateQuestionsByKitAsync(questionKitId, questionsToUpdate);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-questions/{questionKitId}")]
        [SwaggerOperation(Summary = "Xóa các câu hỏi", Description = "Xóa câu hỏi theo Id")]
        public async Task<IActionResult> DeleteQuestionsAsync(string questionKitId, [FromBody] List<string> questionIds)
        {
            var response = await _questionManagerServices.DeleteQuestionsAsync(questionKitId, questionIds);
            return StatusCode(response.StatusCode, response);

        }
    }
}