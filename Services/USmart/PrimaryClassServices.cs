using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using VinhUni_Educator_API.Configs;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class PrimaryClassServices : IPrimaryClassServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PrimaryClassServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public PrimaryClassServices(ApplicationDBContext context, IConfiguration config, ILogger<PrimaryClassServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices, IMapper mapper)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
            _mapper = mapper;
        }
        public async Task<ActionResponse> SyncPrimaryClassesAsync()
        {
            var APIBaseURL = _config["VinhUNISmart:API"];
            if (string.IsNullOrEmpty(APIBaseURL))
            {
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Không thể tìm thấy địa chỉ API của hệ thống USmart trong cấu hình"
                };
            }
            try
            {
                //Check if the last sync action is within 30 minutes
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncClass);
                if (lastSync != null && lastSync.SyncAt.AddMinutes(SyncActionList.SYNC_TIME_OUT) > DateTime.UtcNow)
                {
                    var remainingTime = (lastSync.SyncAt.AddMinutes(SyncActionList.SYNC_TIME_OUT) - DateTime.UtcNow).Minutes;
                    return new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = $"Đã có ai đó thực hiện đồng bộ gần đây, vui lòng đợi {remainingTime} phút trước khi thực hiện lại"
                    };
                }
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("Không thể xác định người dùng");
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new Exception("Không thể xác định người dùng");
                }
                var uSmartToken = await _context.USmartTokens.FirstOrDefaultAsync(t => t.UserId == userId);
                var isTokenExpired = _jwtServices.IsTokenExpired(uSmartToken?.Token);
                if (uSmartToken == null || isTokenExpired)
                {
                    return new ActionResponse
                    {
                        StatusCode = 403,
                        IsSuccess = false,
                        Message = "Bạn không có quyền truy cập vào hệ thống USmart, vui lòng đăng nhập lại bằng tài tài USmart"
                    };
                }
                var fetch = new FetchData(APIBaseURL, uSmartToken.Token);
                var formBody = @"{
                                ""pageInfo"": {
                                    ""page"": 1,
                                    ""pageSize"": 1000
                                },
                                ""sorts"": [],
                                ""filters"": [
                                    {
                                        ""filters"": [],
                                        ""field"": ""idHe"",
                                        ""operator"": ""eq"",
                                        ""value"": ""CQ"" // Đại học chính quy
                                    }
                                ],
                                ""fields"": ""id,code,ten,idNganh,idKhoaHoc"",
                                }";
                var responseData = await fetch.FetchAsync("gwsg/dbnguoihoc/tbl_DanhSach_LopHanhChinh/getPaged", null, formBody, Method.Post);
                List<ClassSyncModel> listClass = JsonSerializer.Deserialize<List<ClassSyncModel>>(responseData?.data?.ToString());
                if (responseData?.success == false || listClass is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy danh sách lớp hành chính từ hệ thống USmart"
                    };
                }
                // Update or insert primary class to database
                int countNewClass = 0;
                int countFailed = 0;
                await _context.Database.BeginTransactionAsync();
                foreach (var item in listClass)
                {
                    var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == item.idKhoaHoc);
                    var program = await _context.TrainingPrograms.FirstOrDefaultAsync(p => p.ProgramCode == item.idNganh);
                    var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(cls => cls.ClassId == item.id);
                    if (program is null || course is null)
                    {
                        countFailed++;
                        continue;
                    }
                    if (primaryClass is null)
                    {
                        primaryClass = new PrimaryClass
                        {
                            ClassId = item.id,
                            ClassCode = item.code,
                            ClassName = item.ten,
                            ProgramId = program.Id,
                            CourseId = course.Id,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _context.PrimaryClasses.AddAsync(primaryClass);
                        countNewClass++;
                    }
                    else
                    {
                        primaryClass.ClassCode = item.code;
                        primaryClass.ClassName = item.ten;
                        primaryClass.ProgramId = program.Id;
                        primaryClass.CourseId = course.Id;
                        _context.PrimaryClasses.Update(primaryClass);
                    }
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncClass,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm {countNewClass} lớp hành chính mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                _context.SaveChanges();
                var message = countNewClass > 0 ? $"Đã cập nhật thêm {countNewClass} lớp mới và {countFailed} chưa được cập nhật" : "Không có lớp nào mới";
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = message,
                };
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred while syncing classes: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetPrimaryClassesAsync(int? pageIndex, int? limit)
        {
            try
            {
                var currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                var query = _context.PrimaryClasses.AsQueryable();
                query = query.Where(cls => cls.IsDeleted == false);
                query = query.OrderByDescending(cls => cls.CreatedAt);
                var classList = await PageList<PrimaryClass, ClassViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy danh sách lớp hành chính thành công",
                    Data = classList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting primary classes: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetDeletedPrimaryClassesAsync(int? pageIndex, int? limit)
        {
            try
            {
                var currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                var query = _context.PrimaryClasses.AsQueryable();
                query = query.Where(cls => cls.IsDeleted == true);
                query = query.OrderByDescending(cls => cls.DeletedAt);
                var classList = await PageList<PrimaryClass, ClassViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy danh sách lớp hành chính đã xóa thành công",
                    Data = classList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting deleted primary classes: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp hành chính đã xóa, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetPrimaryClassByIdAsync(int classId)
        {
            try
            {
                var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(cls => cls.Id == classId && cls.IsDeleted == false);
                if (primaryClass is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp hành chính hoặc lớp hành chính đã bị xóa"
                    };
                }
                var classViewModel = _mapper.Map<ClassViewModel>(primaryClass);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy thông tin lớp hành chính thành công",
                    Data = classViewModel
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting primary class by id: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> DeletePrimaryClassAsync(int classId)
        {
            try
            {
                var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(cls => cls.Id == classId);
                if (primaryClass is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp hành chính"
                    };
                }
                primaryClass.IsDeleted = true;
                primaryClass.DeletedAt = DateTime.UtcNow;
                primaryClass.DeletedBy = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Xóa lớp hành chính thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while deleting primary class: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> RestorePrimaryClassAsync(int classId)
        {
            try
            {
                var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(cls => cls.Id == classId && cls.IsDeleted == true);
                if (primaryClass is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp hành chính"
                    };
                }
                primaryClass.IsDeleted = false;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Khôi phục lớp hành chính thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while restoring primary class: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> UpdatePrimaryClassAsync(int classId, UpdateClassModel model)
        {
            try
            {
                var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(cls => cls.Id == classId && cls.IsDeleted == false);
                var existsClassCode = await _context.PrimaryClasses.AnyAsync(cls => cls.ClassCode == model.ClassCode && cls.Id != classId);
                if (primaryClass is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp hành chính hoặc lớp hành chính đã bị xóa"
                    };
                }
                if (existsClassCode)
                {
                    return new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Mã lớp hành chính đã tồn tại"
                    };
                }
                primaryClass.ClassCode = model.ClassCode ?? primaryClass.ClassCode;
                primaryClass.ClassName = model.ClassName ?? primaryClass.ClassName;
                primaryClass.ProgramId = model.ProgramId ?? primaryClass.ProgramId;
                primaryClass.CourseId = model.CourseId ?? primaryClass.CourseId;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin lớp hành chính thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while updating primary class: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật thông tin lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetPrimaryClassesByProgramAsync(int programId, int? pageIndex, int? limit)
        {
            try
            {
                var currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                var program = await _context.TrainingPrograms.FirstOrDefaultAsync(p => p.Id == programId);
                if (program is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy chương trình đào tạo"
                    };
                }
                var query = _context.PrimaryClasses.AsQueryable();
                query = query.Where(cls => cls.ProgramId == programId && cls.IsDeleted == false);
                query = query.OrderByDescending(cls => cls.CreatedAt);
                var classList = await PageList<PrimaryClass, ClassViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy danh sách lớp hành chính thành công",
                    Data = classList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting primary classes by program: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetPrimaryClassesByCourseAsync(int courseId, int? pageIndex, int? limit)
        {
            try
            {
                var currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
                if (course is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy khóa học"
                    };
                }
                var query = _context.PrimaryClasses.AsQueryable();
                query = query.Where(cls => cls.CourseId == courseId && cls.IsDeleted == false);
                query = query.OrderByDescending(cls => cls.CreatedAt);
                var classList = await PageList<PrimaryClass, ClassViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy danh sách lớp hành chính thành công",
                    Data = classList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting primary classes by course: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> SearchPrimaryClassesAsync(string? keyword, int? limit)
        {
            try
            {
                var currentLimit = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _context.PrimaryClasses.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(cls => cls.ClassCode.Contains(keyword) || cls.ClassName.Contains(keyword) && cls.IsDeleted == false);
                }
                query = query.OrderByDescending(cls => cls.CreatedAt);
                var response = await query.Take(currentLimit).ToListAsync();
                var classList = _mapper.Map<List<ClassViewModel>>(response);
                int totalClassFound = classList.Count;
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = $"Tìm thấy {totalClassFound} lớp hành chính",
                    Data = classList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while searching primary classes: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tìm kiếm lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
    }
}