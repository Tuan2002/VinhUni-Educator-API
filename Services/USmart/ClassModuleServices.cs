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
        public async Task<ActionResponse> GetClassByStudentAsync(int studentId, int semesterId, int? pageIndex = DEFAULT_PAGE_INDEX, int? pageSize = DEFAULT_PAGE_SIZE)
        {
            try
            {
                var currentPage = pageIndex ?? DEFAULT_PAGE_INDEX;
                var currentSize = pageSize ?? DEFAULT_PAGE_SIZE;
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
                var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
                if (student == null || semester == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin sinh viên hoặc học kỳ"
                    };
                }
                var query = _context.ModuleClassStudents.AsQueryable();
                query = query.Where(mcs => mcs.StudentId == studentId && mcs.SemesterId == semesterId);
                var classModulesQuery = query.Select(mcs => mcs.ModuleClass);
                var classModules = await PageList<ModuleClass, ClassModuleViewModel>.CreateWithMapperAsync(classModulesQuery, currentPage, currentSize, _mapper);
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
                _logger.LogError($"Error occurred in ClassModuleServices.GetClassModuleByStudentAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp học phần"
                };
            }
        }
        public async Task<ActionResponse> GetClassModulesAsync(int semesterId, int? pageIndex, int? limit)
        {
            try
            {
                var currentPage = pageIndex ?? DEFAULT_PAGE_INDEX;
                var currentSize = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.ModuleClasses.AsQueryable();
                query = query.Where(mc => mc.SemesterId == semesterId);
                query = query.Where(mc => mc.IsDeleted == false);
                query = query.OrderBy(mc => mc.CreatedAt);
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
                _logger.LogError($"Error occurred in ClassModuleServices.GetClassModulesAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách lớp học phần"
                };
            }
        }
        public async Task<ActionResponse> GetClassModuleAsync(string moduleClassId)
        {
            try
            {
                var moduleClass = await _context.ModuleClasses.FirstOrDefaultAsync(mc => mc.Id == moduleClassId);
                if (moduleClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin lớp học phần"
                    };
                }
                var classModule = _mapper.Map<ModuleClass, ClassModuleViewModel>(moduleClass);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin lớp học phần thành công",
                    Data = classModule
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ClassModuleServices.GetClassModuleAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin lớp học phần"
                };
            }
        }
        public async Task<ActionResponse> SyncClassModuleStudentsAsync(string moduleClassId)
        {
            try
            {
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
                var moduleClass = await _context.ModuleClasses.FirstOrDefaultAsync(mc => mc.Id == moduleClassId);
                if (moduleClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin lớp học phần"
                    };
                }
                var fetch = new FetchData(APIBaseURL, uSmartToken.Token);
                var formBody = @"{
                                ""pageInfo"": {
                                    ""page"": 1,
                                    ""pageSize"": 1000
                                },
                                ""sorts"": [],
                                ""filters"": []
                                }";
                var response = await fetch.FetchAsync($"gwsg/dbnguoihoc/tbl_NguoiHoc_HoSo/getPaged/LopHocPhan/{moduleClass.ModuleClassId}", null, formBody, Method.Post);
                List<StudentSyncModel> students = JsonSerializer.Deserialize<List<StudentSyncModel>>(response?.data?.ToString());
                if (response?.success == false || students is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy danh sách sinh viên lớp học phần từ hệ thống USmart"
                    };
                }
                int countSuccess = 0;
                int countFailed = 0;
                foreach (var item in students)
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == item.id);
                    if (student == null)
                    {
                        var program = await _context.TrainingPrograms.FirstOrDefaultAsync(p => p.ProgramCode == item.idNganh);
                        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == item.idKhoaHoc);
                        var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(pc => pc.ClassId == item.idLopHanhChinh);
                        if (course == null || program == null || primaryClass == null)
                        {
                            countFailed++;
                            continue;
                        }
                        student = new Student
                        {
                            StudentId = item.id,
                            StudentCode = item.code,
                            FirstName = item.ho,
                            LastName = item.ten,
                            Gender = ConvertGender.ConvertToInt(item.gioiTinh),
                            Dob = DateOnly.FromDateTime(item.ngaySinh),
                            ClassId = primaryClass.Id,
                            ProgramId = program.Id,
                            CourseId = course.Id,
                            CreatedAt = DateTime.UtcNow,
                            IsSynced = true,
                            CreatedById = user.Id,
                            SmartId = int.Parse(item.userId)
                        };
                        await _context.Students.AddAsync(student);
                        await _context.SaveChangesAsync();
                    }
                    var existStudent = await _context.ModuleClassStudents.AnyAsync(mcs => mcs.StudentId == student.Id && mcs.ModuleClassId == moduleClass.Id);
                    if (!existStudent)
                    {
                        var newStudent = new ModuleClassStudent
                        {
                            StudentId = student.Id,
                            ModuleClassId = moduleClass.Id,
                            SemesterId = moduleClass.SemesterId,
                            AddedAt = DateTime.UtcNow,
                            AddedById = user.Id

                        };
                        await _context.ModuleClassStudents.AddAsync(newStudent);
                        await _context.SaveChangesAsync();
                        countSuccess++;
                    }
                }
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = countSuccess > 0 ? $"Đồng bộ thành công {countSuccess} sinh viên" : "Không có sinh viên nào được thêm vào lớp học phần",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ClassModuleServices.SyncClassModuleStudentsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ sinh viên lớp học phần"
                };
            }
        }
        public async Task<ActionResponse> GetStudentsByModuleClass(string moduleClassId)
        {
            try
            {
                var moduleClass = await _context.ModuleClasses.FirstOrDefaultAsync(mc => mc.Id == moduleClassId);
                if (moduleClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin lớp học phần"
                    };
                }
                var query = _context.ModuleClassStudents.AsQueryable();
                query = query.Where(mcs => mcs.ModuleClassId == moduleClassId);
                var rawStudents = await query.Select(mcs => mcs.Student).ToListAsync();
                var students = _mapper.Map<List<Student>, List<StudentViewModel>>(rawStudents);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách sinh viên thành công",
                    Data = students
                };

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ClassModuleServices.GetStudentsByModuleClass: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách sinh viên"
                };
            }
        }
    }
}