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
    public class QuestionManagerServices : IQuestionManagerServices
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<QuestionManagerServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public QuestionManagerServices(ApplicationDBContext context, ILogger<QuestionManagerServices> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<ActionResponse> ImportQuestionsAsync(string questionKitId, List<CreateQuestionModel> questions)
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
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(q => q.Id == questionKitId);
                if (questionKit == null || questionKit.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc không quyền thực hiện chức năng này"
                    };
                }
                if (questions == null || questions.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Danh sách câu hỏi không được để trống"
                    };
                }
                int orderIndex = questionKit.Questions.Count;
                int countSuccess = 0;
                int countFailed = 0;
                await _context.Database.BeginTransactionAsync();
                List<CreateQuestionResult> importQuestionErrors = [];
                foreach (var question in questions)
                {
                    var resultItem = _mapper.Map<CreateQuestionResult>(question);
                    try
                    {
                        if (question.Answers.Count < 2)
                        {
                            resultItem.ErrorMessage = "Câu hỏi cần có ít nhất 2 đáp án";
                            importQuestionErrors.Add(resultItem);
                            countFailed++;
                            continue;
                        }
                        if (question.IsMultipleChoice && question.Answers.Count < 2)
                        {
                            resultItem.ErrorMessage = "Câu hỏi nhiều lựa chọn cần có ít nhất 2 đáp án";
                            importQuestionErrors.Add(resultItem);
                            countFailed++;
                            continue;
                        }
                        if (!question.IsMultipleChoice && question.Answers.Count(x => x.IsCorrect) > 1)
                        {
                            resultItem.ErrorMessage = "Câu hỏi đơn lựa chọn chỉ có thể có 1 đáp án đúng";
                            importQuestionErrors.Add(resultItem);
                            countFailed++;
                            continue;
                        }
                        if (!question.Answers.Any(a => a.IsCorrect))
                        {
                            resultItem.ErrorMessage = "Câu hỏi cần có ít nhất 1 đáp án đúng";
                            importQuestionErrors.Add(resultItem);
                            countFailed++;
                            continue;
                        }
                        var newQuestion = new Question
                        {
                            QuestionContent = question.QuestionContent,
                            QuestionNote = question.QuestionNote,
                            QuestionImages = question.QuestionImages,
                            IsMultipleChoice = question.IsMultipleChoice,
                            Level = question.Level,
                            Order = orderIndex,
                            QuestionKitId = questionKitId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        newQuestion.Answers = question.Answers.Select(a => new QuestionAnswer
                        {
                            QuestionId = newQuestion.Id,
                            AnswerContent = a.AnswerContent,
                            AnswerImage = a.AnswerImage,
                            IsCorrect = a.IsCorrect
                        }).ToList();
                        await _context.Questions.AddAsync(newQuestion);
                        orderIndex++;
                        countSuccess++;
                    }
                    catch (Exception ex)
                    {
                        countFailed++;
                        resultItem.ErrorMessage = "Lỗi không xác định hoặc câu hỏi không hợp lệ";
                        importQuestionErrors.Add(resultItem);
                        _logger.LogError($"Error occurred in QuestionManagerServices.ImportQuestionsAsync: {ex.Message} at {DateTime.UtcNow}");
                        continue;
                    }
                }
                questionKit.UpdatedAt = DateTime.UtcNow;
                questionKit.ModifiedById = userId;
                _context.QuestionKits.Update(questionKit);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = $"Nhập thành công {countSuccess} câu hỏi",
                    Data = new
                    {
                        ImportQuestionErrors = importQuestionErrors,
                        CountSuccess = countSuccess,
                        CountFailed = countFailed
                    }
                };
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in QuestionManagerServices.ImportQuestionsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi nhập câu hỏi. Vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetQuestionsNByKitAsync(string questionKitId, int? pageIndex, int? limit)
        {
            try
            {
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
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
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải giáo viên, không có quyền truy cập bộ câu hỏi"
                    };
                }
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(q => q.Id == questionKitId && q.IsDeleted == false);
                if (questionKit == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc bộ câu hỏi đã bị xóa"
                    };
                }
                switch (questionKit.IsShared)
                {
                    case false:
                        if (questionKit.CreatedById != userId)
                        {
                            return new ActionResponse
                            {
                                StatusCode = StatusCodes.Status403Forbidden,
                                IsSuccess = false,
                                Message = "Bạn không có quyền truy cập bộ câu hỏi này"
                            };
                        }
                        var questionQuery = _context.Questions.AsQueryable();
                        questionQuery = questionQuery.Where(q => q.QuestionKitId == questionKitId);
                        questionQuery = questionQuery.OrderBy(q => q.Order);
                        var questions = await PageList<Question, QuestionViewModel>.CreateWithMapperAsync(questionQuery, currentPageIndex, currentLimit, _mapper);
                        return new ActionResponse
                        {
                            StatusCode = StatusCodes.Status200OK,
                            IsSuccess = true,
                            Message = "Lấy danh sách câu hỏi thành công",
                            Data = questions
                        };
                    case true:
                        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == questionKit.CategoryId);
                        if (category == null)
                        {
                            return new ActionResponse
                            {
                                StatusCode = StatusCodes.Status404NotFound,
                                IsSuccess = false,
                                Message = "Không tìm thấy danh mục câu hỏi"
                            };
                        }
                        bool isShared = _context.SharedCategories.Any(sc => sc.CategoryId == category.Id && sc.ViewerId == teacher.Id);
                        if (!isShared)
                        {
                            return new ActionResponse
                            {
                                StatusCode = StatusCodes.Status403Forbidden,
                                IsSuccess = false,
                                Message = "Bộ câu hỏi này đã được chia sẻ nhưng bạn không có quyền truy cập"
                            };
                        }
                        questionQuery = _context.Questions.AsQueryable();
                        questionQuery = questionQuery.Where(q => q.QuestionKitId == questionKitId);
                        questionQuery = questionQuery.OrderBy(q => q.Order);
                        questions = await PageList<Question, QuestionViewModel>.CreateWithMapperAsync(questionQuery, currentPageIndex, currentLimit, _mapper);
                        return new ActionResponse
                        {
                            StatusCode = StatusCodes.Status200OK,
                            IsSuccess = true,
                            Message = "Lấy danh sách câu hỏi thành công",
                            Data = questions
                        };
                    default:
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionManagerServices.GetQuestionsNByKitAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách câu hỏi. Vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetQuestionByIdAsync(string questionId)
        {
            try
            {
                var rawQuestion = await _context.Questions.FirstOrDefaultAsync(q => q.Id == questionId);
                if (rawQuestion == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy câu hỏi"
                    };
                }
                var question = _mapper.Map<QuestionViewModel>(rawQuestion);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin câu hỏi thành công",
                    Data = question
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in QuestionManagerServices.GetQuestionByIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin câu hỏi. Vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> UpdateQuestionsByKitAsync(string questionKitId, List<UpdateQuestionModel> questionsToUpdate)
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
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(q => q.Id == questionKitId);
                if (questionKit == null || questionKit.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc không quyền thực hiện chức năng này"
                    };
                }
                if (questionsToUpdate.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Danh sách câu hỏi không được để trống"
                    };
                }
                int countSuccess = 0;
                _context.Database.BeginTransaction();
                foreach (var question in questionsToUpdate)
                {
                    if (question.Answers.Count < 2)
                        continue;
                    if (!question.Answers.Any(a => a.IsCorrect))
                        continue;
                    if (question.IsMultipleChoice && question.Answers.Count < 2)
                        continue;
                    if (!question.IsMultipleChoice && question.Answers.Count(x => x.IsCorrect) > 1)
                        continue;
                    var questionToUpdate = await _context.Questions.FirstOrDefaultAsync(q => q.Id == question.Id);
                    if (questionToUpdate != null)
                    {
                        questionToUpdate.QuestionContent = question.QuestionContent;
                        questionToUpdate.QuestionNote = question.QuestionNote;
                        questionToUpdate.QuestionImages = question.QuestionImages;
                        questionToUpdate.IsMultipleChoice = question.IsMultipleChoice;
                        questionToUpdate.Level = question.Level;
                        questionToUpdate.Order = question.Order;
                        questionToUpdate.UpdatedAt = DateTime.UtcNow;
                        foreach (var answer in question.Answers)
                        {
                            var answerToUpdate = await _context.QuestionAnswers.FirstOrDefaultAsync(a => a.Id == answer.Id);
                            if (answerToUpdate != null)
                            {
                                answerToUpdate.AnswerContent = answer.AnswerContent;
                                answerToUpdate.AnswerImage = answer.AnswerImage;
                                answerToUpdate.IsCorrect = answer.IsCorrect;
                                _context.QuestionAnswers.Update(answerToUpdate);
                            }
                        }
                        _context.Questions.Update(questionToUpdate);
                        countSuccess++;
                    }
                }
                questionKit.UpdatedAt = DateTime.UtcNow;
                questionKit.ModifiedById = userId;
                _context.QuestionKits.Update(questionKit);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = countSuccess > 0 ? $"Cập nhật thành công {countSuccess} câu hỏi" : "Không có câu hỏi nào được cập nhật"
                };
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in QuestionManagerServices.UpdateQuestionsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật câu hỏi. Vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> DeleteQuestionsAsync(string questionKitId, List<string> questionsToDelete)
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
                var questionKit = await _context.QuestionKits.FirstOrDefaultAsync(q => q.Id == questionKitId);
                if (questionKit == null || questionKit.CreatedById != userId)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy bộ câu hỏi hoặc không quyền thực hiện chức năng này"
                    };
                }
                if (questionsToDelete.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Danh sách câu hỏi không được để trống"
                    };
                }
                int countSuccess = 0;
                _context.Database.BeginTransaction();
                foreach (var questionId in questionsToDelete)
                {
                    var questionToDelete = await _context.Questions.FirstOrDefaultAsync(q => q.Id == questionId);
                    if (questionToDelete != null)
                    {
                        _context.Questions.Remove(questionToDelete);
                        countSuccess++;
                    }
                }
                questionKit.UpdatedAt = DateTime.UtcNow;
                questionKit.ModifiedById = userId;
                _context.QuestionKits.Update(questionKit);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = countSuccess > 0 ? $"Xóa thành công {countSuccess} câu hỏi" : "Không có câu hỏi nào được xóa"
                };
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in QuestionManagerServices.DeleteQuestionsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa câu hỏi. Vui lòng thử lại sau"
                };
            }
        }
    }
}