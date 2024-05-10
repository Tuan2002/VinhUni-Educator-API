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
    public class ExamManagerServices : IExamManagerServices
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<ExamManagerServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public ExamManagerServices(ApplicationDBContext context, ILogger<ExamManagerServices> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<ActionResponse> CreateExamAsync(CreateExamModel model)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần phải đăng nhập để thực hiện chức năng này"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải giáo viên, không thể tạo đề thi"
                    };
                }
                var newExam = new Exam
                {
                    ExamName = model.ExamName,
                    ExamDescription = model.ExamDescription,
                    IsPublished = model.IsPublished,
                    CreatedById = userId,
                    OwnerId = teacher.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Database.BeginTransactionAsync();
                await _context.Exams.AddAsync(newExam);
                if (model.QuestionIds != null && model.QuestionIds.Count > 0)
                {
                    foreach (var questionId in model.QuestionIds)
                    {
                        var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == questionId);
                        if (question == null)
                            continue;
                        var examQuestion = new ExamQuestion
                        {
                            ExamId = newExam.Id,
                            QuestionId = question.Id,
                            QuestionKitId = question.QuestionKitId,
                            AddedAt = DateTime.UtcNow
                        };
                        await _context.ExamQuestions.AddAsync(examQuestion);
                    }
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Tạo đề thi thành công",
                    Data = _mapper.Map<ExamViewModel>(newExam)
                };
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in ExamManagerServices.CreateExamAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tạo đề thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetExamsAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần phải đăng nhập để thực hiện chức năng này"
                    };
                }
                var query = _context.Exams.AsQueryable();
                query = query.Where(x => x.IsDeleted == false);
                query = query.Where(x => x.CreatedById == userId);
                query = query.OrderByDescending(x => x.CreatedAt);
                var exams = await PageList<Exam, ExamViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách đề thi thành công",
                    Data = exams
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamManagerServices.GetExamsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách đề thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> UpdateExamAsync(string examId, UpdateExamModel model)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần phải đăng nhập để thực hiện chức năng này"
                    };
                }
                var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
                if (exam == null || exam.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đề thi hoặc đề thi đã bị xóa"
                    };
                }
                if (exam.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền chỉnh sửa đề thi này"
                    };
                }
                exam.ExamName = model.ExamName ?? exam.ExamName;
                exam.ExamDescription = model.ExamDescription ?? exam.ExamDescription;
                exam.IsPublished = model.IsPublished ?? exam.IsPublished;
                exam.UpdatedAt = DateTime.UtcNow;
                _context.Exams.Update(exam);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật đề thi thành công",
                    Data = _mapper.Map<ExamViewModel>(exam)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamManagerServices.UpdateExamAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật đề thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> DeleteExamAsync(string examId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần phải đăng nhập để thực hiện chức năng này"
                    };
                }
                var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
                if (exam == null || exam.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đề thi hoặc đề thi đã bị xóa"
                    };
                }
                if (exam.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền xóa đề thi này"
                    };
                }
                exam.IsDeleted = true;
                _context.Exams.Update(exam);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa đề thi thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamManagerServices.DeleteExamAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa đề thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetQuestionsByExamAsync(string examId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần phải đăng nhập để thực hiện chức năng này"
                    };
                }
                var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
                if (exam == null || exam.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đề thi hoặc đề thi đã bị xóa"
                    };
                }
                if (exam.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền xem câu hỏi trong đề thi này"
                    };
                }
                var query = _context.ExamQuestions.AsQueryable();
                query = query.Where(x => x.ExamId == examId);
                query = query.OrderByDescending(x => x.AddedAt);
                var questions = await query.Select(x => x.Question).ToListAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách câu hỏi của đề thi thành công",
                    Data = new
                    {
                        ExamInfo = _mapper.Map<ExamViewModel>(exam),
                        Questions = _mapper.Map<List<QuestionViewModel>>(questions)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamManagerServices.GetQuestionsInExamAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách câu hỏi trong đề thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> AddQuestionsToExamAsync(string examId, List<string> questionIds)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần phải đăng nhập để thực hiện chức năng này"
                    };
                }
                var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
                if (exam == null || exam.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đề thi hoặc đề thi đã bị xóa"
                    };
                }
                if (exam.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền thêm câu hỏi vào đề thi này"
                    };
                }
                var importableQuestionIds = questionIds.Where(x => !exam.ExamQuestions.Any(eq => eq.QuestionId == x)).ToList();
                int countSuccess = 0;
                await _context.Database.BeginTransactionAsync();
                foreach (var questionId in importableQuestionIds)
                {
                    var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == questionId);
                    if (question == null)
                        continue;
                    var examQuestion = new ExamQuestion
                    {
                        ExamId = exam.Id,
                        QuestionId = question.Id,
                        QuestionKitId = question.QuestionKitId,
                        AddedAt = DateTime.UtcNow
                    };
                    exam.ExamQuestions.Add(examQuestion);
                    countSuccess++;
                }
                exam.UpdatedAt = DateTime.UtcNow;
                _context.Exams.Update(exam);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = $"Đã thêm {countSuccess} câu hỏi vào đề thi",
                    Data = _mapper.Map<ExamViewModel>(exam)
                };
            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                _logger.LogError($"Error occurred in ExamManagerServices.AddQuestionsToExamAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi thêm câu hỏi vào đề thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> RemoveQuestionsFromExamAsync(string examId, List<string> questionIds)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần phải đăng nhập để thực hiện chức năng này"
                    };
                }
                var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
                if (exam == null || exam.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đề thi hoặc đề thi đã bị xóa"
                    };
                }
                if (exam.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền xóa câu hỏi khỏi đề thi này"
                    };
                }
                var removableQuestions = exam.ExamQuestions.Where(eq => questionIds.Contains(eq.QuestionId)).ToList();
                await _context.Database.BeginTransactionAsync();
                _context.ExamQuestions.RemoveRange(removableQuestions);
                exam.UpdatedAt = DateTime.UtcNow;
                _context.Exams.Update(exam);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = $"Đã xóa {removableQuestions.Count} câu hỏi khỏi đề thi",
                    Data = _mapper.Map<ExamViewModel>(exam)
                };
            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                _logger.LogError($"Error occurred in ExamManagerServices.RemoveQuestionsFromExamAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa câu hỏi khỏi đề thi, vui lòng thử lại sau!"
                };
            }
        }
    }
}