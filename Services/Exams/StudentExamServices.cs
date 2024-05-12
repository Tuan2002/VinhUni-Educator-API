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
    public class StudentExamServices : IStudentExamServices
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<StudentExamServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 20;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public StudentExamServices(ApplicationDBContext context, ILogger<StudentExamServices> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<ActionResponse> GetExamSeasonAsync(string examSeasonCode)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thực hiện thao tác này",
                    };
                }
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.SeasonCode == examSeasonCode);
                if (examSeason == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin kỳ thi",
                    };
                }
                int usedTurns = await _context.ExamTurns.CountAsync(x => x.ExamParticipant.StudentId == student.Id && x.ExamSeasonId == examSeason.Id);
                var examSeasonVM = _mapper.Map<StudentSeasonViewModel>(examSeason);
                examSeasonVM.RemainingRetryTurn = examSeason.MaxRetryTurn - usedTurns;
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin kỳ thi thành công",
                    Data = examSeasonVM,
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred in StudentExamServices.GetExamSeasonAsync: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin kỳ thi",
                };
            }
        }
        public async Task<ActionResponse> GetExamTurnsAsync(string examSeasonCode)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thực hiện thao tác này",
                    };
                }
                var query = _context.ExamTurns.AsQueryable();
                query = query.Where(x => x.ExamSeason.SeasonCode == examSeasonCode);
                query = query.Where(x => x.ExamParticipant.StudentId == student.Id);
                query = query.OrderBy(ex => ex.StartAt);
                var rawExamTurns = await query.ToListAsync();
                var examTurns = _mapper.Map<List<StudentExamTurnModel>>(rawExamTurns);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách lượt thi thành công",
                    Data = examTurns,
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred in StudentExamServices.GetExamTurnsAsync: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lượt thi",
                };
            }
        }
        public async Task<ActionResponse> StartExamAsync(string examSeasonCode, string moduleClassId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thaam gia thi",
                    };
                }
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.SeasonCode == examSeasonCode);
                if (examSeason == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi",
                    };
                }
                var assignedClass = await _context.ExamAssignedClasses.FirstOrDefaultAsync(x => x.ModuleClassId == moduleClassId && x.ExamSeasonId == examSeason.Id);
                if (assignedClass == null || !assignedClass.ModuleClass.ModuleClassStudents.Any(x => x.StudentId == student.Id))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không được phép tham gia kỳ thi này",
                    };
                }
                if (examSeason.StartTime > DateTime.UtcNow)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi chưa bắt đầu, không thể tham gia",
                    };
                }
                if (examSeason.EndTime < DateTime.UtcNow || examSeason.IsFinished)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi đã kết thúc, không thể tham gia",
                    };
                }
                if (DateTime.UtcNow.AddMinutes(examSeason.DurationInMinutes) > examSeason.EndTime)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi sắp kết thúc, không thể tham gia",
                    };
                }
                var examParticipant = await _context.ExamParticipants.FirstOrDefaultAsync(x => x.ExamSeasonId == examSeason.Id && x.StudentId == student.Id);
                if (!examSeason.AllowRetry && examParticipant != null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi không cho phép thi lại",
                    };
                }
                if (examParticipant != null && examParticipant.ExamTurns.Any(x => !x.IsFinished))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Bạn chưa hoàn thành lượt thi trước đó, vui lòng hoàn thành trước khi bắt đầu lượt mới",
                    };
                }
                if (examParticipant != null && examParticipant.ExamTurns.Count == examSeason.MaxRetryTurn)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Bạn đã hết số lượt thi lại",
                    };
                }
                if (examParticipant == null)
                {
                    examParticipant = new ExamParticipant
                    {
                        ExamSeasonId = examSeason.Id,
                        AssignedClassId = assignedClass.Id,
                        StudentId = student.Id,
                        JoinedAt = DateTime.UtcNow,
                    };
                    await _context.ExamParticipants.AddAsync(examParticipant);
                }
                int lastTurn = _context.ExamTurns.Count(x => x.ExamParticipantId == examParticipant.Id);
                var newExamTurn = new ExamTurn
                {
                    ExamSeasonId = examSeason.Id,
                    ExamParticipantId = examParticipant.Id,
                    TurnNumber = lastTurn + 1,
                    StartAt = DateTime.UtcNow,
                };
                await _context.ExamTurns.AddAsync(newExamTurn);
                await _context.SaveChangesAsync();
                var newExamSession = new StudentExamSessionModel
                {
                    ExamSeasonCode = examSeason.SeasonCode,
                    ExamSeasonName = examSeason.SeasonName,
                    StudentCode = student.StudentCode,
                    StudentName = student.GetFullName(),
                    TurnId = newExamTurn.Id,
                    TurnNumber = newExamTurn.TurnNumber,
                    StartAt = newExamTurn.StartAt,
                    EndAt = newExamTurn.StartAt.AddMinutes(examSeason.DurationInMinutes)
                };
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Bắt đầu lượt thi mới thành công",
                    Data = newExamSession,
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred in StudentExamServices.StartExamAsync: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi bắt đầu lượt thi",
                };
            }
        }
        public async Task<ActionResponse> ForceFinishExamTurnAsync(string examSeasonCode, string turnId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thực hiện thao tác này",
                    };
                }
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.SeasonCode == examSeasonCode);
                if (examSeason == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin kỳ thi",
                    };
                }
                var examTurn = await _context.ExamTurns.FirstOrDefaultAsync(x => x.Id == turnId && x.ExamParticipant.StudentId == student.Id && x.ExamSeasonId == examSeason.Id);
                if (examTurn == null || examTurn.IsFinished)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lượt thi hoặc lượt thi đã hoàn thành",
                    };
                }
                var examResult = new ExamResult
                {
                    ExamTurnId = examTurn.Id,
                    TotalPoint = 0,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _context.ExamResults.AddAsync(examResult);
                examTurn.IsFinished = true;
                examTurn.CompletedAt = DateTime.UtcNow;
                _context.ExamTurns.Update(examTurn);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Kết thúc lượt thi thành công, bạn sẽ nhận điểm 0 cho lượt thi này",
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred in StudentExamServices.ForceFinishExamTurnAsync: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi kết thúc lượt thi",
                };
            }
        }
        public async Task<ActionResponse> GetExamQuestionsAsync(string seasonCode, int? pageIndex, int? limit)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thực hiện thao tác này",
                    };
                }
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.SeasonCode == seasonCode);
                if (examSeason == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi",
                    };
                }
                var isValidStudent = await _context.ExamParticipants.AnyAsync(x => x.ExamSeasonId == examSeason.Id && x.StudentId == student.Id);
                if (!isValidStudent)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không được phép tham gia kỳ thi này",
                    };
                }
                var currentExam = examSeason.Exam;
                if (currentExam == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin đề thi",
                    };
                }
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.ExamQuestions.AsQueryable();
                query = query.Where(x => x.ExamId == currentExam.Id);
                var questionQuery = query.Select(x => x.Question);
                var questions = await PageList<Question, StudentQuestionViewModel>.CreateWithMapperAsync(questionQuery, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách câu hỏi thành công",
                    Data = questions,
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred in StudentExamServices.GetExamQuestionsAsync: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách câu hỏi",
                };
            }
        }
        public async Task<ActionResponse> SubmitExamAnswersAsync(string seasonCode, string turnId, List<SubmitQuestionModel> submitQuestions)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thực hiện thao tác này",
                    };
                }
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.SeasonCode == seasonCode);
                if (examSeason == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin kỳ thi",
                    };
                }
                var examParticipant = await _context.ExamParticipants.FirstOrDefaultAsync(x => x.ExamSeasonId == examSeason.Id && x.StudentId == student.Id);
                if (examParticipant == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Bạn chưa tham gia kỳ thi này",
                    };
                }
                var examTurn = await _context.ExamTurns.FirstOrDefaultAsync(x => x.Id == turnId && examParticipant.StudentId == student.Id && x.ExamSeasonId == examSeason.Id);
                if (examTurn == null || examTurn.IsFinished || examTurn.StartAt.AddMinutes(examSeason.DurationInMinutes) < DateTime.UtcNow.AddMinutes(1))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin lượt thi hoặc lượt thi đã kết thúc",
                    };
                }
                var currentExam = examSeason.Exam;
                if (currentExam == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin đề thi",
                    };
                }
                var totalQuestions = currentExam.ExamQuestions.Count;
                var validExamQuestions = submitQuestions.Where(x => currentExam.ExamQuestions.Any(q => q.QuestionId == x.QuestionId)).ToList();
                var examResult = new ExamResult
                {
                    ExamTurnId = examTurn.Id,
                    TotalPoint = 0,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _context.Database.BeginTransactionAsync();
                await _context.ExamResults.AddAsync(examResult);
                await _context.SaveChangesAsync();
                var examResultDetails = new List<ExamResultDetail>();
                int countCorrectAnswers = 0;
                foreach (var submitQuestion in validExamQuestions)
                {
                    var question = _context.Questions.FirstOrDefault(x => x.Id == submitQuestion.QuestionId);
                    if (question == null)
                    {
                        continue;
                    }
                    var answer = question.Answers.FirstOrDefault(x => x.Id == submitQuestion.AnswerId);
                    if (answer != null)
                    {
                        switch (answer.IsCorrect)
                        {
                            case true:
                                countCorrectAnswers++;
                                examResultDetails.Add(new ExamResultDetail
                                {
                                    ExamResultId = examResult.Id,
                                    QuestionId = question.Id,
                                    SelectedAnswerId = answer.Id,
                                    IsCorrect = true,
                                });
                                break;
                            case false:
                                examResultDetails.Add(new ExamResultDetail
                                {
                                    ExamResultId = examResult.Id,
                                    QuestionId = question.Id,
                                    SelectedAnswerId = answer.Id,
                                    IsCorrect = false,
                                });
                                break;
                        }
                    }
                }
                decimal POINT_PERQUESION = 10 / totalQuestions;
                decimal totalPoint = Math.Round(POINT_PERQUESION * countCorrectAnswers, 2);
                examResult.TotalPoint = totalPoint;
                examResult.UpdatedAt = DateTime.UtcNow;
                examTurn.IsFinished = true;
                examTurn.CompletedAt = DateTime.UtcNow;
                await _context.ExamResultDetails.AddRangeAsync(examResultDetails);
                _context.ExamResults.Update(examResult);
                _context.ExamTurns.Update(examTurn);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                // Show point and result
                var examResultVM = new ExamResultViewModel
                {
                    Id = examResult.Id,
                    ExamSeasonCode = examSeason.SeasonCode,
                    ExamSeasonName = examSeason.SeasonName,
                    StudentCode = student.StudentCode,
                    StudentName = student.GetFullName(),
                    TurnId = examTurn.Id,
                    TurnNumber = examTurn.TurnNumber,
                    StartAt = examTurn.StartAt,
                    TotalPoint = totalPoint,
                    CompletedAt = examTurn.CompletedAt,
                };
                examResultVM.TotalPoint = examSeason.ShowPoint ? examResultVM.TotalPoint : null;
                examResultVM.CorrectAnswers = examSeason.ShowResult ? countCorrectAnswers : null;
                examResultVM.TotalQuestions = totalQuestions;
                examResultVM.ResultQuestions = examSeason.ShowResult ? _mapper.Map<List<StudentQuestionResult>>(examResultDetails) : null;
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Nộp bài thi thành công",
                    Data = examResultVM
                };
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in StudentExamServices.SubmitExamAnswersAsync: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi nộp bài thi",
                };
            }
        }
        public async Task<ActionResponse> GetExamResultAsync(string seasonCode, string turnId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thực hiện thao tác này",
                    };
                }
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.SeasonCode == seasonCode);
                if (examSeason == null || !examSeason.ShowResult)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin kỳ thi hoặc kỳ thi không hiển thị kết quả",
                    };
                }
                var examTurn = await _context.ExamTurns.FirstOrDefaultAsync(x => x.Id == turnId && x.ExamParticipant.StudentId == student.Id && x.ExamSeasonId == examSeason.Id);
                if (examTurn == null || !examTurn.IsFinished)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin lượt thi hoặc lượt thi chưa hoàn thành",
                    };
                }
                var rawExamResult = await _context.ExamResults.FirstOrDefaultAsync(x => x.ExamTurnId == examTurn.Id);
                if (rawExamResult == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin kết quả thi",
                    };
                }
                var rawExamResultDetails = rawExamResult.ExamResultDetails.ToList();
                var examResult = _mapper.Map<ExamResultViewModel>(rawExamResult);
                if (rawExamResultDetails.Count > 0)
                    examResult.ResultQuestions = _mapper.Map<List<StudentQuestionResult>>(rawExamResultDetails);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy kết quả thi thành công",
                    Data = examResult,
                };

            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred in StudentExamServices.GetExamResultAsync: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy kết quả thi",
                };
            }
        }
        public async Task<ActionResponse> ResumeExamTurnAsync(string seasonCode, string turnId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Vui lòng đăng nhập để thực hiện thao tác này",
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không phải là sinh viên, không thể thực hiện thao tác này",
                    };
                }
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.SeasonCode == seasonCode);
                if (examSeason == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin kỳ thi",
                    };
                }
                var examTurn = await _context.ExamTurns.FirstOrDefaultAsync(x => x.Id == turnId && x.ExamParticipant.StudentId == student.Id && x.ExamSeasonId == examSeason.Id);
                if (examTurn == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin lượt thi",
                    };
                }
                if (examTurn.IsFinished || examTurn.StartAt.AddMinutes(examSeason.DurationInMinutes) < DateTime.UtcNow)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Lượt thi đã hoàn thành hoặc đã hết thời gian, không thể tiếp tục lượt thi",
                    };
                }
                var newExamSession = new StudentExamSessionModel
                {
                    ExamSeasonCode = examSeason.SeasonCode,
                    ExamSeasonName = examSeason.SeasonName,
                    StudentCode = student.StudentCode,
                    StudentName = student.GetFullName(),
                    TurnId = examTurn.Id,
                    TurnNumber = examTurn.TurnNumber,
                    StartAt = examTurn.StartAt,
                    EndAt = examTurn.StartAt.AddMinutes(examSeason.DurationInMinutes)
                };
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Tiếp tục lượt thi thành công",
                    Data = newExamSession,
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred in StudentExamServices.ResumeExamTurn: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tiếp tục lượt thi",
                };
            }
        }
    }
}