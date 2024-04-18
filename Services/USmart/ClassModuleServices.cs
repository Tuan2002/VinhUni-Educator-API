using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class ClassModuleServices : IClassModuleServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<ClassModuleServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public ClassModuleServices(ApplicationDBContext context, IConfiguration config, ILogger<ClassModuleServices> logger, IHttpContextAccessor httpContextAccessor, IJwtServices jwtServices, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtServices = jwtServices;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<ActionResponse> SyncClassModulesByTeacherIdAsync(int teacherId, int semesterId)
        {
            try
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId);
                var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
                var schoolYearCode = semester?.SchoolYear.YearCode;
                if (teacher == null || semester == null || schoolYearCode == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Tìm không thấy thông tin giảng viên hoặc học kỳ"
                    };
                }
                var APIBaseURL = _config["VinhUNISmart:API"];
                if (string.IsNullOrEmpty(APIBaseURL))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Không thể tìm thấy địa chỉ API của hệ thống USmart trong cấu hình"
                    };
                }
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new Exception("Không thể xác định người dùng");
                var uSmartToken = await _context.USmartTokens.FirstOrDefaultAsync(t => t.UserId == userId);
                var isTokenExpired = _jwtServices.IsTokenExpired(uSmartToken?.Token);
                if (uSmartToken == null || isTokenExpired)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền truy cập vào hệ thống USmart, vui lòng đăng nhập lại bằng tài khoản USmart"
                    };
                }
                var fetch = new FetchData(APIBaseURL, uSmartToken.Token);
                var formBody = @"{
                    ""type"": 2,
                    ""lstTeacherCode"": [
                        """ + teacher.TeacherCode + @"""
                    ],
                    ""idHe"": 1,
                    ""idNamHoc"": " + schoolYearCode + @",
                    ""idHocKy"": " + semester.SemesterId + @",
                }";
                var response = await fetch.FetchAsync("gwsg/dbdaotao_chinhquy/tbl_Tkb_LopHocPhan_LichHoc/GetDataForExportTkb", null, formBody, Method.Post);
                TimeTableSyncModel timeTable = JsonSerializer.Deserialize<TimeTableSyncModel>(response?.data?.ToString());
                if (response?.success == false || timeTable is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy thời khóa biểu giảng viên từ hệ thống USmart"
                    };
                }
                if (timeTable.lstExportTkbRowData is null || timeTable.lstExportTkbRowData.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thời khóa biểu giảng viên trong hệ thống USmart"
                    };
                }
                int countSuccess = 0;
                int countFailed = 0;
                await _context.Database.BeginTransactionAsync();
                foreach (var item in timeTable.lstExportTkbRowData)
                {
                    var classId = item.idLopHocPhan;
                    try
                    {
                        var existClass = await _context.ModuleClasses.AnyAsync(mc => mc.ModuleClassId == classId);
                        if (existClass)
                        {
                            continue;
                        }
                        var responseData = await fetch.FetchAsync($"gwsg/dbdaotao_chinhquy/tbl_Tkb_LopHocPhan/{classId}");
                        ClassModuleSyncModel classData = JsonSerializer.Deserialize<ClassModuleSyncModel>(responseData?.data?.ToString());
                        if (responseData?.success == false || classData is null)
                        {
                            throw new Exception("Có lỗi xảy ra khi lấy thông tin lớp học phần từ hệ thống USmart");
                        }
                        var module = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleId == classData.idHocPhan || m.ModuleCode == item.maHp);
                        if (module == null)
                        {
                            throw new Exception("Không tìm thấy thông tin học phần trong hệ thống");
                        }
                        var newClass = new ModuleClass
                        {
                            ModuleClassId = classData.id,
                            ModuleClassCode = classData.code,
                            ModuleClassName = classData.ten,
                            ModuleId = module.Id,
                            TeacherId = teacher.Id,
                            SemesterId = semester.Id,
                            MaxStudents = classData.soSvDangKyToiDa,
                            CreatedAt = DateTime.UtcNow
                        };
                        if (classData.idLopHocPhanGoc != null)
                        {
                            var parentClass = await _context.ModuleClasses.FirstOrDefaultAsync(mc => mc.ModuleClassId == classData.idLopHocPhanGoc);
                            if (parentClass != null)
                            {
                                newClass.IsChildClass = true;
                                newClass.ParentClassId = parentClass.Id;
                            }
                        }
                        await _context.ModuleClasses.AddAsync(newClass);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        countFailed++;
                        _logger.LogError($"Error occurred in ClassModuleServices.SyncModulesByTeacherIdAsync: {ex.Message} at {DateTime.UtcNow}");
                        continue;
                    }
                    countSuccess++;
                }
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = countSuccess > 0 ? $"Đồng bộ thành công {countSuccess} lớp học phần" : "Không có lớp học phần nào được đồng bộ",
                };

            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in ClassModuleServices.SyncModulesByTeacherIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ lớp học phần từ hệ thống USmart"
                };
            }
        }
        public async Task<ActionResponse> SyncClassModulesByTeacher(int semesterId)
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
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new Exception("Không thể xác định người dùng");
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains(AppRoles.Teacher))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Tài khoản không phải là giảng viên"
                    };
                }
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
                var schoolYearCode = semester?.SchoolYear.YearCode;
                if (semester == null || schoolYearCode == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin học kỳ"
                    };
                }
                var response = await SyncClassModulesByTeacherIdAsync(teacher.Id, semesterId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ClassModuleServices.SyncClassModulesByTeacher: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ lớp học phần"
                };
            }
        }
        public async Task<ActionResponse> GetClassByTeacherAsync(int teacherId, int semesterId, int? pageIndex = DEFAULT_PAGE_INDEX, int? pageSize = DEFAULT_PAGE_SIZE)
        {
            try
            {
                var currentPage = pageIndex ?? DEFAULT_PAGE_INDEX;
                var currentSize = pageSize ?? DEFAULT_PAGE_SIZE;
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId);
                var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
                if (teacher == null || semester == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên hoặc học kỳ"
                    };
                }
                var query = _context.ModuleClasses.AsQueryable();
                query = query.Where(mc => mc.TeacherId == teacherId && mc.SemesterId == semesterId);
                query = query.Where(mc => mc.IsDeleted == false);
                var classModules = await PageList<ModuleClass, ClassModuleViewModel>.CreateWithMapperAsync(query, currentPage, currentSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách lớp học phần thành công",
                    Data = classModules
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ClassModuleServices.GetClassByTeacherAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp học phần"
                };
            }
        }
    }
}