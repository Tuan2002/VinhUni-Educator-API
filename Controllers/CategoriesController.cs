using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Configs;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize(Roles = AppRoles.Teacher)]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý danh mục")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryServices _categoryServices;
        public CategoriesController(ICategoryServices categoryServices)
        {
            _categoryServices = categoryServices;
        }
        [HttpGet]
        [Route("my-categories")]
        [SwaggerOperation(Summary = "Lấy danh sách danh mục của tôi")]
        public async Task<IActionResult> GetMyCategories(int? pageIndex, int? limit)
        {
            var response = await _categoryServices.GetMyCategories(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("shared-categories")]
        [SwaggerOperation(Summary = "Lấy danh sách danh mục được chia sẻ với tôi")]
        public async Task<IActionResult> GetMySharedCategories(int? pageIndex, int? limit)
        {
            var response = await _categoryServices.GetMySharedCategoriesAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("sharing-categories")]
        [SwaggerOperation(Summary = "Lấy danh sách danh mục tôi đang chia sẻ")]
        public async Task<IActionResult> GetMySharingCategories(int? pageIndex, int? limit)
        {
            var response = await _categoryServices.GetMySharingCategoriesAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("create-category")]
        [SwaggerOperation(Summary = "Tạo danh mục mới")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryModel model)
        {
            var response = await _categoryServices.CreateCategoryAsync(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-category/{categoryId}")]
        [SwaggerOperation(Summary = "Lấy thông tin danh mục theo ID")]
        public async Task<IActionResult> GetCategoryById(string categoryId)
        {
            var response = await _categoryServices.GetCategoryByIdAsync(categoryId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-category/{categoryId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin danh mục")]
        public async Task<IActionResult> UpdateCategory(string categoryId, [FromBody] UpdateCategoryModel model)
        {
            var response = await _categoryServices.UpdateCategoryAsync(categoryId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-category/{categoryId}")]
        [SwaggerOperation(Summary = "Xóa danh mục")]
        public async Task<IActionResult> DeleteCategory(string categoryId)
        {
            var response = await _categoryServices.DeleteCategoryAsync(categoryId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-shared-category/{categoryId}")]
        [SwaggerOperation(Summary = "Xóa danh mục được chia sẻ")]
        public async Task<IActionResult> DeleteSharedCategory(string categoryId)
        {
            var response = await _categoryServices.DeleteSharedCategoryAsync(categoryId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("share-category/{categoryId}")]
        [SwaggerOperation(Summary = "Chia sẻ danh mục")]
        public async Task<IActionResult> ShareCategory(string categoryId, [FromBody] ShareCategoryModel model)
        {
            var response = await _categoryServices.ShareCategoryAsync(categoryId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("unshare-category/{categoryId}")]
        [SwaggerOperation(Summary = "Hủy chia sẻ danh mục")]
        public async Task<IActionResult> UnshareCategory(string categoryId, [FromBody] ShareCategoryModel teachers)
        {
            var response = await _categoryServices.UnShareCategoryAsync(categoryId, teachers);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-teachers-shared/{categoryId}")]
        [SwaggerOperation(Summary = "Lấy danh sách giáo viên đã chia sẻ danh mục")]
        public async Task<IActionResult> GetTeachersShared(string categoryId)
        {
            var response = await _categoryServices.GetTeachersSharedAsync(categoryId);
            return StatusCode(response.StatusCode, response);
        }
    }
}