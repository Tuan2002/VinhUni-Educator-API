using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RandomString4Net;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class ExamSeasonServices : IExamSeasonServices
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<ExamSeasonServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public ExamSeasonServices(ApplicationDBContext context, ILogger<ExamSeasonServices> logger, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<ActionResponse> CreateExamSeasonAsync(CreateSeasonModel model)
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
                        Message = "Bạn không phải giáo viên, không thể tạo kỳ thi"
                    };
                }
                var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == model.ExamId);
                if (exam == null || exam.IsDeleted || !exam.IsPublished)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đề thi hoặc đề thi chưa được công bố"
                    };
                }
                var semester = await _context.Semesters.FirstOrDefaultAsync(x => x.Id == model.SemesterId);
                if (semester == null || semester.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học kỳ hoặc học kỳ đã bị xóa"
                    };
                }
                if (model.EndTime < model.StartTime)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Thời gian kết thúc phải sau thời gian bắt đầu"
                    };
                }
                var assignableClasses = await _context.ModuleClasses.Where(x => model.ModuleClassIds.Contains(x.Id)).ToListAsync();
                if (assignableClasses.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học phần thuộc trong danh sách"
                    };
                }
                var newSeason = new ExamSeason
                {
                    SeasonName = model.SeasonName,
                    Description = model.Description,
                    Password = model.Password ?? RandomString.GetString(Types.NUMBERS, 6),
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    DurationInMinutes = model.DurationInMinutes,
                    SemesterId = model.SemesterId,
                    ExamId = model.ExamId,
                    UsePassword = model.UsePassword,
                    ShowResult = model.ShowResult,
                    ShowPoint = model.ShowPoint,
                    AllowRetry = model.AllowRetry,
                    MaxRetryTurn = model.MaxRetryTurn ?? 1,
                    CreatedById = userId,
                    OwnerId = teacher.Id,
                    CreatedAt = DateTime.UtcNow,
                };
                var existingSeason = await _context.ExamSeasons.AnyAsync(es => es.SeasonCode == newSeason.SeasonCode);
                if (existingSeason)
                {
                    newSeason.SeasonCode = RandomString.GetString(Types.ALPHANUMERIC_UPPERCASE, 16);
                }
                await _context.Database.BeginTransactionAsync();
                await _context.ExamSeasons.AddAsync(newSeason);
                foreach (var assignedClass in assignableClasses)
                {
                    var examAssignedClass = new ExamAssignedClass
                    {
                        ExamSeasonId = newSeason.Id,
                        ModuleClassId = assignedClass.Id,
                        AddedAt = DateTime.UtcNow,
                    };
                    await _context.ExamAssignedClasses.AddAsync(examAssignedClass);
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status201Created,
                    IsSuccess = true,
                    Message = "Tạo kỳ thi thành công",
                    Data = _mapper.Map<ExamSeasonViewModel>(newSeason)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.CreateExamSeasonAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tạo kỳ thi mới, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> ChangeExamAsync(string examSeasonId, string examId)
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
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId && x.CreatedById == userId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                if (examSeason.StartTime < DateTime.UtcNow || examSeason.IsFinished || examSeason.EndTime < DateTime.UtcNow)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi đã bắt đầu hoặc đã kết thúc, không thể thay đổi đề thi"
                    };
                }
                var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == examId);
                if (exam == null || exam.IsDeleted || !exam.IsPublished)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đề thi hoặc đề thi chưa được công bố"
                    };
                }
                examSeason.ExamId = examId;
                examSeason.UpdatedAt = DateTime.UtcNow;
                _context.ExamSeasons.Update(examSeason);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Thay đổi đề thi thành công",
                    Data = _mapper.Map<ExamSeasonDetailModel>(examSeason)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.ChangeExamAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi thay đổi đề thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> UpdateExamSeasonAsync(string examSeasonId, UpdateSeasonModel model)
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
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId && x.CreatedById == userId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                if (examSeason.StartTime < DateTime.UtcNow || examSeason.IsFinished || examSeason.EndTime < DateTime.UtcNow)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi đã bắt đầu hoặc đã kết thúc, không thể cập nhật thông tin"
                    };
                }
                var semester = await _context.Semesters.FirstOrDefaultAsync(x => x.Id == model.SemesterId);
                if (semester == null || semester.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học kỳ hoặc học kỳ đã bị xóa"
                    };
                }
                if (model.EndTime < model.StartTime)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Thời gian kết thúc phải sau thời gian bắt đầu"
                    };
                }
                examSeason.SeasonName = model.SeasonName ?? examSeason.SeasonName;
                examSeason.Description = model.Description ?? examSeason.Description;
                examSeason.Password = model.Password ?? examSeason.Password;
                examSeason.StartTime = model.StartTime ?? examSeason.StartTime;
                examSeason.EndTime = model.EndTime ?? examSeason.EndTime;
                examSeason.DurationInMinutes = model.DurationInMinutes ?? examSeason.DurationInMinutes;
                examSeason.SemesterId = model.SemesterId ?? examSeason.SemesterId;
                examSeason.UsePassword = model.UsePassword ?? examSeason.UsePassword;
                examSeason.ShowResult = model.ShowResult ?? examSeason.ShowResult;
                examSeason.ShowPoint = model.ShowPoint ?? examSeason.ShowPoint;
                examSeason.AllowRetry = model.AllowRetry ?? examSeason.AllowRetry;
                examSeason.MaxRetryTurn = model.MaxRetryTurn ?? examSeason.MaxRetryTurn;
                examSeason.UpdatedAt = DateTime.UtcNow;
                _context.ExamSeasons.Update(examSeason);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin kỳ thi thành công",
                    Data = _mapper.Map<ExamSeasonDetailModel>(examSeason)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.UpdateExamSeasonAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật thông tin kỳ thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetExamSeasonsByClassAsync(string moduleClassId, int? pageIndex, int? limit)
        {
            try
            {
                var classModule = await _context.ModuleClasses.FirstOrDefaultAsync(x => x.Id == moduleClassId);
                if (classModule == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học phần"
                    };
                }
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.ExamSeasons.AsQueryable();
                query = query.Where(x => x.SemesterId == classModule.SemesterId);
                query = query.Where(x => x.AssignedClasses.Any(ac => ac.ModuleClassId == moduleClassId));
                query = query.Where(x => x.IsDeleted == false);
                var examSeasons = await PageList<ExamSeason, ExamSeasonViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách kỳ thi thành công",
                    Data = examSeasons
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.GetExamSeasonsByClassAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách kỳ thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetExamSeasonByIdAsync(string examSeasonId)
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
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId && x.CreatedById == userId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin kỳ thi thành công",
                    Data = _mapper.Map<ExamSeasonDetailModel>(examSeason)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.GetExamSeasonByIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin kỳ thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetAssignClassAsync(string examSeasonId)
        {
            try
            {
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                var assignedClasses = await _context.ExamAssignedClasses.Where(x => x.ExamSeasonId == examSeasonId).ToListAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách lớp học phần thành công",
                    Data = _mapper.Map<List<AssignedClassViewModel>>(assignedClasses)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.GetAssignClassAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp học phần, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> AddClassToExamSeasonAsync(string examSeasonId, List<string> moduleClassIds)
        {
            try
            {
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                if (examSeason.IsFinished || examSeason.EndTime < DateTime.UtcNow)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi đã kết thúc, không thể thêm lớp học phần"
                    };
                }
                var unAssignedClassIds = moduleClassIds.Where(x => !examSeason.AssignedClasses.Any(ac => ac.ModuleClassId == x)).ToList();
                var assignableClasses = await _context.ModuleClasses.Where(x => unAssignedClassIds.Contains(x.Id)).ToListAsync();
                if (assignableClasses.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học phần trong danh sách"
                    };
                }
                await _context.Database.BeginTransactionAsync();
                foreach (var assignedClass in assignableClasses)
                {
                    var examAssignedClass = new ExamAssignedClass
                    {
                        ExamSeasonId = examSeason.Id,
                        ModuleClassId = assignedClass.Id,
                        AddedAt = DateTime.UtcNow,
                    };
                    await _context.ExamAssignedClasses.AddAsync(examAssignedClass);
                }
                examSeason.UpdatedAt = DateTime.UtcNow;
                _context.ExamSeasons.Update(examSeason);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Thêm lớp học phần vào kỳ thi thành công"
                };
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in ExamSeasonServices.AddClassToExamSeasonAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi thêm lớp học phần vào kỳ thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> RemoveClassFromExamSeasonAsync(string examSeasonId, List<string> moduleClassIds)
        {
            try
            {
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                if (examSeason.IsFinished || examSeason.EndTime < DateTime.UtcNow)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi đã kết thúc, không thể xóa lớp học phần"
                    };
                }
                var assignedClasses = await _context.ExamAssignedClasses.Where(x => x.ExamSeasonId == examSeasonId && moduleClassIds.Contains(x.ModuleClassId)).ToListAsync();
                if (assignedClasses.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học phần trong danh sách"
                    };
                }
                await _context.Database.BeginTransactionAsync();
                _context.ExamAssignedClasses.RemoveRange(assignedClasses);
                examSeason.UpdatedAt = DateTime.UtcNow;
                _context.ExamSeasons.Update(examSeason);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa lớp học phần khỏi kỳ thi thành công"
                };
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in ExamSeasonServices.RemoveClassFromExamSeasonAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa lớp học phần khỏi kỳ thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> ForceFinishExamSeasonAsync(string examSeasonId)
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
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId && x.CreatedById == userId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                if (examSeason.IsFinished || examSeason.EndTime < DateTime.UtcNow)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Kỳ thi đã kết thúc"
                    };
                }
                examSeason.IsFinished = true;
                examSeason.UpdatedAt = DateTime.UtcNow;
                _context.ExamSeasons.Update(examSeason);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Đã kết thúc kỳ thi thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.ForceFinishExamSeasonAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi kết thúc kỳ thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> DeleteExamSeasonAsync(string examSeasonId, bool foreverDelete = false)
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
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId && x.CreatedById == userId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi"
                    };
                }
                if (foreverDelete)
                {
                    _context.ExamSeasons.Remove(examSeason);
                    await _context.SaveChangesAsync();
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        IsSuccess = true,
                        Message = "Xóa kỳ thi thành công"
                    };
                }
                examSeason.IsDeleted = true;
                examSeason.UpdatedAt = DateTime.UtcNow;
                _context.ExamSeasons.Update(examSeason);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa kỳ thi thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.DeleteExamSeasonAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa kỳ thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetParticipantsBySeasonAsync(string examSeasonId, string moduleClassId, int? pageIndex, int? limit)
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
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId && x.CreatedById == userId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                var assignedClass = await _context.ExamAssignedClasses.FirstOrDefaultAsync(x => x.ExamSeasonId == examSeasonId && x.ModuleClassId == moduleClassId);
                if (assignedClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Lớp học phần không thuộc kỳ thi này"
                    };
                }
                int currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                int currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.ExamParticipants.AsQueryable();
                query = query.Where(x => x.ExamSeasonId == examSeasonId);
                query = query.Where(x => x.AssignedClassId == assignedClass.Id);
                var participants = await PageList<ExamParticipant, ExamParticipantViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách thí sinh thành công",
                    Data = participants
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.GetParticipantsBySeasonAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách thí sinh, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetStudentExamTurnsAsync(string examSeasonId, int studentId)
        {
            try
            {
                var examSeason = await _context.ExamSeasons.FirstOrDefaultAsync(x => x.Id == examSeasonId);
                if (examSeason == null || examSeason.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kỳ thi hoặc kỳ thi đã bị xóa"
                    };
                }
                var participant = await _context.ExamParticipants.FirstOrDefaultAsync(x => x.ExamSeasonId == examSeasonId && x.StudentId == studentId);
                if (participant == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thí sinh trong kỳ thi"
                    };
                }
                var rawExamTurns = await _context.ExamTurns.Where(x => x.ExamParticipantId == participant.Id).ToListAsync();
                var examTurns = _mapper.Map<List<ExamTurnViewModel>>(rawExamTurns);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách lượt thi thành công",
                    Data = examTurns
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.GetStudentExamTurnsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lượt thi, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> GetStudentExamResultAsync(string turnId)
        {
            try
            {
                var examTurn = await _context.ExamTurns.FirstOrDefaultAsync(x => x.Id == turnId);
                if (examTurn == null || !examTurn.IsFinished)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin lượt thi hoặc lượt thi chưa hoàn thành"
                    };
                }
                var rawExamResult = await _context.ExamResults.FirstOrDefaultAsync(x => x.ExamTurnId == turnId);
                if (rawExamResult == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy kết quả thi"
                    };
                }
                var rawExamResultDetails = rawExamResult.ExamResultDetails.ToList();
                var examResult = _mapper.Map<ExamResultViewModel>(rawExamResult);
                _ = rawExamResultDetails.Count > 0 ? examResult.ResultQuestions = _mapper.Map<List<StudentQuestionResult>>(rawExamResultDetails) : null;
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy kết quả thi thành công",
                    Data = examResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ExamSeasonServices.GetStudentExamResult: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy kết quả thi, vui lòng thử lại sau!"
                };
            }
        }

    }
}