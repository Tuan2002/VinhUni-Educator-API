using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class CategoryServices : ICategoryServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<CategoryServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public CategoryServices(ApplicationDBContext context, IConfiguration config, ILogger<CategoryServices> logger, IHttpContextAccessor httpContextAccessor, IJwtServices jwtServices, IMapper mapper)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtServices = jwtServices;
            _mapper = mapper;
        }
        public async Task<ActionResponse> GetMyCategories(int? pageIndex, int? limit)
        {
            try
            {
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var query = _context.Categories.AsQueryable();
                query = query.Where(x => x.OwnerId == teacher.Id);
                query = query.Where(x => x.IsDeleted == false);
                query = query.OrderByDescending(x => x.CreatedAt);
                var categories = await PageList<Category, CategoryViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách danh mục thành công",
                    Data = categories
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.GetMyCategories: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách danh mục, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetMySharingCategoriesAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var query = _context.SharedCategories.AsQueryable();
                query = query.Where(sc => sc.SharedById == teacher.Id);
                query = query.OrderByDescending(sc => sc.SharedAt);
                var categoryQuery = query.Select(sc => sc.Category).Where(c => c != null);
                // Remove duplicate categories
                categoryQuery = categoryQuery.GroupBy(c => c!.Id).Select(c => c.FirstOrDefault()!);
                var categories = await PageList<Category, CategoryViewModel>.CreateWithMapperAsync(categoryQuery, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách danh mục đã chia sẻ thành công",
                    Data = categories
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.GetMySharingCategory: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách danh mục đã chia sẻ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetMySharedCategoriesAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var query = _context.SharedCategories.AsQueryable();
                query = query.Where(sc => sc.ViewerId == teacher.Id);
                query = query.Where(sc => sc.SharedUntil == null || sc.SharedUntil >= DateOnly.FromDateTime(DateTime.UtcNow));
                query = query.OrderByDescending(sc => sc.SharedAt);
                var categoryQuery = query.Select(sc => sc.Category);
                var categories = await PageList<Category, CategoryViewModel>.CreateWithMapperAsync(categoryQuery, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách danh mục được chia sẻ thành công",
                    Data = categories
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.GetSharedCategoriesAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách danh mục được chia sẻ, vui lòng thử lại sau"
                };

            }
        }
        public async Task<ActionResponse> CreateCategoryAsync(CreateCategoryModel model)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var category = new Category
                {
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    CreatedById = userId,
                    OwnerId = teacher.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status201Created,
                    IsSuccess = true,
                    Message = "Tạo danh mục thành công",
                    Data = _mapper.Map<CategoryViewModel>(category)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.CreateCategoryAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tạo danh mục, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetCategoryByIdAsync(string categoryId)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null || category.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục hoặc danh mục đã bị xóa"
                    };
                }
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin danh mục thành công",
                    Data = _mapper.Map<CategoryViewModel>(category)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.GetCategoryByIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin danh mục, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> UpdateCategoryAsync(string categoryId, UpdateCategoryModel model)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null || category.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục hoặc danh mục đã bị xóa"
                    };
                }
                if (category.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền cập nhật danh mục này"
                    };
                }
                category.CategoryName = model.CategoryName ?? category.CategoryName;
                category.Description = model.Description ?? category.Description;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật danh mục thành công",
                    Data = _mapper.Map<CategoryViewModel>(category)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.UpdateCategoryAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật danh mục, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> DeleteCategoryAsync(string categoryId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null || category.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục hoặc danh mục đã bị xóa"
                    };
                }
                if (category.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền xóa danh mục này"
                    };
                }
                category.IsDeleted = true;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa danh mục thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.DeleteCategoryAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa danh mục, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> DeleteSharedCategoryAsync(string categoryId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null || category.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục hoặc danh mục đã bị xóa"
                    };
                }
                var sharedCategory = await _context.SharedCategories.FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.ViewerId == teacher.Id);
                if (sharedCategory == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Danh mục không được chia sẻ cho bạn"
                    };
                }
                _context.SharedCategories.Remove(sharedCategory);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xoá danh mục được chia sẻ thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.DeleteSharedCategory: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi danh mục được chia sẻ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> ShareCategoryAsync(string categoryId, ShareCategoryModel model)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var owner = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (owner == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null || category.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục hoặc danh mục đã bị xóa"
                    };
                }
                if (category.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền chia sẻ danh mục này"
                    };
                }
                if (model.TeacherIds == null || model.TeacherIds.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Vui lòng chọn ít nhất một giảng viên để chia sẻ"
                    };
                }
                int countShared = 0;
                await _context.Database.BeginTransactionAsync();
                foreach (var teacherId in model.TeacherIds)
                {
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId);
                    if (teacher == null || teacher.IsDeleted || teacher.Id == owner.Id)
                    {
                        continue;
                    }
                    var shared = await _context.SharedCategories.FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.ViewerId == teacher.Id);
                    if (shared != null && (shared.SharedUntil >= DateOnly.FromDateTime(DateTime.UtcNow) || shared.SharedUntil == null))
                    {
                        shared.SharedAt = DateTime.UtcNow;
                        shared.SharedUntil = model.ShareUntil;
                        _context.SharedCategories.Update(shared);
                        countShared++;
                        continue;
                    }
                    var sharedCategory = new SharedCategory
                    {
                        CategoryId = categoryId,
                        ViewerId = teacher.Id,
                        SharedById = owner.Id,
                        SharedAt = DateTime.UtcNow,
                        SharedUntil = model.ShareUntil
                    };
                    await _context.SharedCategories.AddAsync(sharedCategory);
                    countShared++;
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = $"Chia sẻ danh mục thành công cho {countShared} giảng viên"
                };
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in CategoryServices.ShareCategoryAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi chia sẻ danh mục, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> UnShareCategoryAsync(string categoryId, ShareCategoryModel model)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này"
                    };
                }
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null || category.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục hoặc danh mục đã bị xóa"
                    };
                }
                if (category.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền hủy chia sẻ danh mục này"
                    };
                }
                if (model.TeacherIds == null || model.TeacherIds.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Vui lòng chọn ít nhất một giảng viên để hủy chia sẻ"
                    };
                }
                int countUnShared = 0;
                foreach (var teacherId in model.TeacherIds)
                {
                    var sharedCategory = await _context.SharedCategories.FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.ViewerId == teacherId);
                    if (sharedCategory != null)
                    {
                        _context.SharedCategories.Remove(sharedCategory);
                        countUnShared++;
                    }
                }
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = $"Hủy chia sẻ danh mục thành công cho {countUnShared} giảng viên"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.UnShareCategoryAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi hủy chia sẻ danh mục, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetTeachersSharedAsync(string categoryId)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null || category.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục hoặc danh mục đã bị xóa"
                    };
                }
                var query = _context.SharedCategories.AsQueryable();
                query = query.Where(sc => sc.CategoryId == categoryId);
                query = query.OrderByDescending(sc => sc.SharedAt);
                query = query.Where(sc => sc.SharedUntil == null || sc.SharedUntil >= DateOnly.FromDateTime(DateTime.UtcNow));
                var rawTeachers = await query.ToListAsync();
                var teachers = _mapper.Map<List<SharedCategoryViewModel>>(rawTeachers);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách giảng viên đã chia sẻ thành công",
                    Data = teachers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in CategoryServices.GetTeachersSharedAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách giảng viên đã chia sẻ, vui lòng thử lại sau"
                };
            }
        }
    }
}