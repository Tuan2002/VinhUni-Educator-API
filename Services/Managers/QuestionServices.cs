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
    public class QuestionServices : IQuestionServices
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<QuestionServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public QuestionServices(ApplicationDBContext context, ILogger<QuestionServices> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<ActionResponse> CreateQuestionKitAsync(CreateQuestionKitModel questionKit)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var owner = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (owner == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là giáo viên, không thể tạo bộ câu hỏi"
                    };
                }
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == questionKit.CategoryId && x.OwnerId == owner.Id);
                if (category == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục câu hỏi hoặc bạn không có quyền truy cập"
                    };
                }
                var newQuestionKit = new QuestionKit
                {
                    KitName = questionKit.KitName,
                    KitDescription = questionKit.KitDescription,
                    Tag = questionKit.Tag,
                    CategoryId = questionKit.CategoryId,
                    OwnerId = owner.Id,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ModifiedById = userId
                };
                await _context.QuestionKits.AddAsync(newQuestionKit);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Tạo bộ câu hỏi thành công",
                    Data = _mapper.Map<QuestionKitViewModel>(newQuestionKit)
                };

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionServices.CreateQuestionKitAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tạo bộ câu hỏi, vui lòng thử lại sau"
                };

            }
        }
        public async Task<ActionResponse> GetQuestionKitsAsync(string categoryId, int? pageIndex, int? limit)
        {
            try
            {
                int pageSize = limit ?? DEFAULT_PAGE_SIZE;
                int currentPage = pageIndex ?? DEFAULT_PAGE_INDEX;
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là giáo viên, không thể xem bộ câu hỏi"
                    };
                }
                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
                if (category == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục câu hỏi"
                    };
                }
                bool isOwner = category.OwnerId == teacher.Id;
                if (!isOwner)
                {
                    var isShared = await _context.SharedCategories.AnyAsync(x => x.CategoryId == categoryId && x.ViewerId == teacher.Id);
                    if (!isShared)
                    {
                        return new ActionResponse
                        {
                            StatusCode = StatusCodes.Status403Forbidden,
                            IsSuccess = false,
                            Message = "Bạn không có quyền xem bộ câu hỏi này"
                        };
                    }
                    var _query = _context.QuestionKits.AsQueryable();
                    _query = _query.Where(x => x.CategoryId == categoryId);
                    _query = _query.Where(x => x.IsShared == true && x.IsDeleted == false);
                    _query = _query.OrderByDescending(x => x.CreatedAt);
                    var _questionKits = await PageList<QuestionKit, QuestionKitViewModel>.CreateWithMapperAsync(_query, currentPage, pageSize, _mapper);
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        IsSuccess = true,
                        Message = "Lấy danh sách bộ câu hỏi thành công",
                        Data = _questionKits
                    };
                }
                var query = _context.QuestionKits.AsQueryable();
                query = query.Where(x => x.CategoryId == categoryId);
                query = query.Where(x => x.IsDeleted == false);
                query = query.OrderByDescending(x => x.CreatedAt);
                var questionKits = await PageList<QuestionKit, QuestionKitViewModel>.CreateWithMapperAsync(query, currentPage, pageSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách bộ câu hỏi thành công",
                    Data = questionKits
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionServices.GetQuestionKitsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bộ câu hỏi, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetQuestionKitByIdAsync(string questionKitId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là giáo viên, không thể xem bộ câu hỏi"
                    };
                }
                var isOwner = await _context.QuestionKits.AnyAsync(x => x.Id == questionKitId && x.OwnerId == teacher.Id);
                if (!isOwner)
                {
                    var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(x => x.Id == questionKitId && x.IsShared == true && x.IsDeleted == false);
                    if (questionKit == null)
                    {
                        return new ActionResponse
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            IsSuccess = false,
                            Message = "Không tìm thấy bộ câu hỏi hoặc bộ câu hỏi đã bị xóa"
                        };
                    }
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        IsSuccess = true,
                        Message = "Lấy thông tin bộ câu hỏi thành công",
                        Data = _mapper.Map<QuestionKitViewModel>(questionKit)
                    };
                }
                var _questionKit = await _context.QuestionKits.FirstOrDefaultAsync(x => x.Id == questionKitId && x.IsDeleted == false);
                if (_questionKit == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc bộ câu hỏi đã bị xóa"
                    };
                }
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin bộ câu hỏi thành công",
                    Data = _mapper.Map<QuestionKitViewModel>(_questionKit)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionServices.GetQuestionKitByIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin bộ câu hỏi, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> UpdateQuestionKitAsync(string questionKitId, UpdateQuestionKitModel model)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là giáo viên, không thể cập nhật bộ câu hỏi"
                    };
                }
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(x => x.Id == questionKitId && x.OwnerId == teacher.Id);
                if (questionKit == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc bạn không có quyền truy cập"
                    };
                }
                var isOwnerCategory = await _context.Categories.AnyAsync(x => x.Id == model.CategoryId && x.OwnerId == teacher.Id);
                if (!isOwnerCategory)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy danh mục câu hỏi hoặc bạn không có quyền truy cập"
                    };
                }
                questionKit.KitName = model.KitName ?? questionKit.KitName;
                questionKit.KitDescription = model.KitDescription ?? questionKit.KitDescription;
                questionKit.Tag = model.Tag;
                questionKit.UpdatedAt = DateTime.UtcNow;
                questionKit.CategoryId = model.CategoryId ?? questionKit.CategoryId;
                questionKit.ModifiedById = userId;
                _context.QuestionKits.Update(questionKit);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin bộ câu hỏi thành công",
                    Data = _mapper.Map<QuestionKitViewModel>(questionKit)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionServices.UpdateQuestionKitAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật bộ câu hỏi, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> DeleteQuestionKitAsync(string questionKitId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(x => x.Id == questionKitId && x.CreatedById == userId);
                if (questionKit == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc bạn không có quyền truy cập"
                    };
                }
                questionKit.IsDeleted = true;
                _context.QuestionKits.Update(questionKit);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa bộ câu hỏi thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionServices.DeleteQuestionKitAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa bộ câu hỏi, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> ShareQuestionKitAsync(string questionKitId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(x => x.Id == questionKitId && x.CreatedById == userId);
                if (questionKit == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc bạn không có quyền truy cập"
                    };
                }
                questionKit.IsShared = true;
                _context.QuestionKits.Update(questionKit);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Chia sẻ bộ câu hỏi thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionServices.ShareQuestionKitAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi chia sẻ bộ câu hỏi, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> UnShareQuestionKitAsync(string questionKitId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(x => x.Id == questionKitId && x.CreatedById == userId);
                if (questionKit == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc bạn không có quyền truy cập"
                    };
                }
                questionKit.IsShared = false;
                _context.QuestionKits.Update(questionKit);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Hủy chia sẻ bộ câu hỏi thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionServices.UnShareQuestionKitAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi hủy chia sẻ bộ câu hỏi, vui lòng thử lại sau"
                };
            }
        }
    }
}