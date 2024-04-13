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
    public class StudentServices : IStudentServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<CourseServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public StudentServices(ApplicationDBContext context, IConfiguration config, ILogger<CourseServices> logger, IHttpContextAccessor httpContextAccessor, IJwtServices jwtServices, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtServices = jwtServices;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<ActionResponse> GetStudentsAsync(int? pageIndex, int? limit)
        {
            try
            {
                var pageSize = limit ?? DEFAULT_PAGE_SIZE;
                var pageNumber = pageIndex ?? DEFAULT_PAGE_INDEX;
                var query = _context.Students.AsQueryable();
                query = query.Where(x => x.IsDeleted == false);
                query = query.OrderByDescending(x => x.CreatedAt);
                var studentList = await PageList<Student, StudentViewModel>.CreateWithMapperAsync(query, pageNumber, pageSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách sinh viên thành công",
                    Data = studentList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting students: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetDeletedStudentsAsync(int? pageIndex, int? limit)
        {
            try
            {
                var pageSize = limit ?? DEFAULT_PAGE_SIZE;
                var pageNumber = pageIndex ?? DEFAULT_PAGE_INDEX;
                var query = _context.Students.AsQueryable();
                query = query.Where(x => x.IsDeleted == true);
                query = query.OrderByDescending(x => x.CreatedAt);
                var studentList = await PageList<Student, StudentViewModel>.CreateWithMapperAsync(query, pageNumber, pageSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách sinh viên đã xoá thành công",
                    Data = studentList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting deleted students: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách sinh viên đã xoá, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetAImportableStudentsByClassAsync(int classId)
        {
            try
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
                var smartToken = _context.USmartTokens.FirstOrDefault(x => x.UserId == userId);
                var isTokenExpired = _jwtServices.IsTokenExpired(smartToken?.Token);
                if (smartToken == null || isTokenExpired)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập bằng tài khoản SSO để thực hiện chức năng này"
                    };
                }
                var primaryClass = _context.PrimaryClasses.FirstOrDefault(x => x.Id == classId);
                if (primaryClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học này trong hệ thống"
                    };
                }
                var fetch = new FetchData(APIBaseURL, smartToken.Token);
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
                                        ""value"": ""CQ""
                                    },
                                    {
                                        ""filters"": [],
                                        ""field"": ""idLopHanhChinh"",
                                        ""operator"": ""eq"",
                                        ""value"": " + primaryClass.ClassId + @"
                                    }      
                                ],
                                }";
                var response = await fetch.FetchAsync("gwsg/dbnguoihoc/tbl_NguoiHoc_HoSo/getPaged/Find", null, formBody, Method.Post);
                List<StudentSyncModel> studentList = JsonSerializer.Deserialize<List<StudentSyncModel>>(response?.data?.ToString());
                if (response?.success == false || studentList == null)
                {
                    throw new Exception("Cannot get students from USmart");
                }
                var importableStudents = studentList.Where(x => !_context.Students.Any(y => y.StudentCode == x.code)).ToList();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách sinh viên có thể nhập thành công",
                    Data = _mapper.Map<List<ImportStudentModel>>(importableStudents)
                };

            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting importable students: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách sinh viên có thể nhập, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> ImportStudentByClass(int classId, List<ImportStudentModel> students)
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
                var primaryClass = _context.PrimaryClasses.FirstOrDefault(x => x.Id == classId);
                if (students == null || students.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Danh sách sinh viên rỗng"
                    };
                }
                int successCount = 0;
                int failCount = 0;
                List<ImportStudentViewModel> importResult = [];
                await _context.Database.BeginTransactionAsync();
                foreach (var student in students)
                {
                    var resultItem = _mapper.Map<ImportStudentViewModel>(student);
                    var studentExist = _context.Students.Any(s => s.StudentCode == student.StudentCode);
                    var course = _context.Courses.FirstOrDefault(c => c.CourseCode == student.CourseCode);
                    var program = _context.TrainingPrograms.FirstOrDefault(p => p.ProgramCode == student.ProgramCode);
                    if (studentExist || student.ClassId != primaryClass?.ClassId)
                    {
                        resultItem = _mapper.Map<ImportStudentViewModel>(student);
                        resultItem.IsImported = false;
                        resultItem.ClassName = primaryClass?.ClassName;
                        resultItem.CourseName = course?.CourseName;
                        resultItem.ProgramName = program?.ProgramName;
                        resultItem.ErrorMessage = "Sinh viên đã tồn tại hoặc không thuộc lớp học này";
                        importResult.Add(resultItem);
                        failCount++;
                        continue;
                    }
                    if (course == null || program == null || primaryClass == null)
                    {
                        resultItem.IsImported = false;
                        resultItem.ErrorMessage = "Không tìm thấy khoa học hoặc ngành học";
                        importResult.Add(resultItem);
                        failCount++;
                        continue;
                    }
                    var newStudent = new Student
                    {
                        StudentId = student.StudentId,
                        StudentCode = student.StudentCode,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        Dob = student.Dob,
                        Gender = student.Gender,
                        ClassId = primaryClass.Id,
                        CourseId = course.Id,
                        ProgramId = program.Id,
                        CreatedById = userId,
                        CreatedAt = DateTime.UtcNow,
                        SmartId = student.SSOId,
                        IsSynced = false
                    };
                    await _context.Students.AddAsync(newStudent);
                    resultItem.IsImported = true;
                    resultItem.ClassName = primaryClass.ClassName;
                    resultItem.CourseName = course.CourseName;
                    resultItem.ProgramName = program.ProgramName;
                    importResult.Add(resultItem);
                    successCount++;
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Nhập danh sách sinh viên thành công",
                    Data = new
                    {
                        SuccessCount = successCount,
                        FailCount = failCount,
                        ImportResult = importResult
                    }
                };
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred while importing students: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi nhập sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetStudentsByClassAsync(int classId)
        {
            try
            {
                var primaryClass = _context.PrimaryClasses.FirstOrDefault(x => x.Id == classId);
                if (primaryClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học này trong hệ thống"
                    };
                }
                var rawStudents = await _context.Students.Where(x => x.ClassId == classId && x.IsDeleted == false).ToListAsync();
                var students = _mapper.Map<List<StudentViewModel>>(rawStudents);
                int countStudent = students.Count;
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách sinh viên thành công",
                    Data = new
                    {
                        Students = students,
                        TotalStudent = countStudent
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting students: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetStudentByCodeAsync(string studentCode)
        {
            try
            {
                var rawStudent = await _context.Students.FirstOrDefaultAsync(x => x.StudentCode == studentCode && x.IsDeleted == false);
                if (rawStudent == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                var student = _mapper.Map<StudentViewModel>(rawStudent);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin sinh viên thành công",
                    Data = student
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting student: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetStudentByIdAsync(int studentId)
        {
            try
            {
                var rawStudent = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.IsDeleted == false);
                if (rawStudent == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                var student = _mapper.Map<StudentViewModel>(rawStudent);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin sinh viên thành công",
                    Data = student
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting student: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> UpdateStudentAsync(int studentId, UpdateStudentModel model)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.IsDeleted == false);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                var primaryClass = _context.PrimaryClasses.FirstOrDefault(x => x.Id == model.ClassId);
                var course = _context.Courses.FirstOrDefault(x => x.Id == model.CourseId);
                var program = _context.TrainingPrograms.FirstOrDefault(x => x.Id == model.ProgramId);
                if (course == null || program == null || primaryClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy khoá học hoặc ngành học"
                    };
                }
                if (program.CourseId != course.Id)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = $"Chương trình đào tạo '{program.ProgramName}' không thuộc khoá học '{course.CourseName}'"
                    };
                }
                if (primaryClass.CourseId != course.Id)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = $"Lớp '{primaryClass.ClassName}' không thuộc khoá học '{course.CourseName}'"
                    };
                }
                if (primaryClass.ProgramId != program.Id)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = $"Lớp '{primaryClass.ClassName}' không thuộc chương trình đào tạo '{program.ProgramName}'"
                    };
                }
                student.FirstName = model.FirstName ?? student.FirstName;
                student.LastName = model.LastName ?? student.LastName;
                student.Dob = model.Dob ?? student.Dob;
                student.Gender = model.Gender ?? student.Gender;
                student.ClassId = model.ClassId ?? student.ClassId;
                student.CourseId = model.CourseId ?? student.CourseId;
                student.ProgramId = model.ProgramId ?? student.ProgramId;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin sinh viên thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while updating student: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật thông tin sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> DeleteStudentAsync(int studentId)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.IsDeleted == false);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                student.IsDeleted = true;
                student.DeletedAt = DateTime.UtcNow;
                student.DeletedBy = userId;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa sinh viên thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while deleting student: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> RestoreStudentAsync(int studentId)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.IsDeleted == true);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                student.IsDeleted = false;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Khôi phục sinh viên thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while restoring student: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục sinh viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> LinkUserAccountAsync(int studentId, string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = $"Không tìm thấy người dùng"
                    };
                }
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(AppRoles.Student))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Không thể liên kết tài khoản người dùng không phải là sinh viên"
                    };
                }
                var linkedStudent = await _context.Students.FirstOrDefaultAsync(x => x.Id != studentId && x.UserId == userId);
                if (linkedStudent != null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = $"Tài khoản người dùng đã được liên kết với sinh viên: {linkedStudent.StudentCode}"
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.IsDeleted == false);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                if (student.UserId != null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Sinh viên này đã liên kết với tài khoản người dùng"
                    };
                }
                if (student.SmartId == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Sinh viên chưa được đồng bộ thông tin từ hệ thống USmart"
                    };
                }
                student.UserId = userId;
                user.USmartId = student.SmartId;
                _context.Students.Update(student);
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Liên kết tài khoản sinh viên thành công",
                    Data = _mapper.Map<StudentViewModel>(student)
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while linking user account: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi liên kết tài khoản, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> UnlinkUserAccountAsync(int studentId)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == student.UserId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Sinh viên không liên kết với tài khoản người dùng nào"
                    };
                }
                student.UserId = null;
                user.USmartId = null;
                _context.Students.Update(student);
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Hủy liên kết tài khoản sinh viên thành công",
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while unlinking user account: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi hủy liên kết tài khoản, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> SearchStudentAsync(string? searchKey, int? limit)
        {
            try
            {
                var pageSize = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _context.Students.AsQueryable();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    query = query.Where(x => x.StudentCode.Contains(searchKey) || x.FirstName.Contains(searchKey) || x.LastName.Contains(searchKey));
                }
                query = query.Where(x => x.IsDeleted == false);
                query = query.OrderByDescending(x => x.CreatedAt);
                var rawStudent = await query.ToListAsync();
                var studentList = _mapper.Map<List<StudentViewModel>>(rawStudent);
                int totalCount = studentList.Count;
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = $"Tìm thấy {totalCount} sinh viên",
                    Data = studentList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while searching students: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tìm kiếm sinh viên, vui lòng thử lại sau",
                };
            }
        }
    }
}