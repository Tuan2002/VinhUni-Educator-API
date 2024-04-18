using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class ClassManagerServices : IClassManagerServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<ClassManagerServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public ClassManagerServices(ApplicationDBContext context, IConfiguration config, ILogger<ClassManagerServices> logger, IHttpContextAccessor httpContextAccessor, IJwtServices jwtServices, IMapper mapper)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtServices = jwtServices;
            _mapper = mapper;
        }
        public async Task<ActionResponse> AddStudentToClassAsync(string moduleClassId, int studentId)
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
                var moduleClass = await _context.ModuleClasses.FirstOrDefaultAsync(x => x.Id == moduleClassId);
                if (moduleClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học phần"
                    };
                }
                var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId);
                if (student == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy sinh viên"
                    };
                }
                var existStudent = await _context.ModuleClassStudents.AnyAsync(x => x.ModuleClassId == moduleClassId && x.StudentId == student.Id);
                if (existStudent)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Sinh viên đã tồn tại trong lớp học phần"
                    };
                }
                var moduleClassStudent = new ModuleClassStudent
                {
                    ModuleClassId = moduleClassId,
                    StudentId = student.Id,
                    SemesterId = moduleClass.SemesterId,
                    AddedAt = DateTime.UtcNow,
                    AddedById = userId
                };
                await _context.ModuleClassStudents.AddAsync(moduleClassStudent);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Thêm sinh viên vào lớp học phần thành công",
                    Data = _mapper.Map<Student, StudentViewModel>(student)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ClassManagerServices.AddStudentToClassAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra trong quá trình thêm sinh viên vào lớp học phần"
                };
            }
        }
        public async Task<ActionResponse> RemoveStudentFromClassAsync(string moduleClassId, int studentId)
        {
            try
            {
                var moduleClass = await _context.ModuleClasses.FirstOrDefaultAsync(x => x.Id == moduleClassId);
                if (moduleClass == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy lớp học phần"
                    };
                }
                var moduleClassStudent = await _context.ModuleClassStudents.FirstOrDefaultAsync(x => x.ModuleClassId == moduleClassId && x.StudentId == studentId);
                if (moduleClassStudent == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Sinh viên không tồn tại trong lớp học phần"
                    };
                }
                _context.ModuleClassStudents.Remove(moduleClassStudent);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa sinh viên khỏi lớp học phần thành công",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ClassManagerServices.RemoveStudentFromClassAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra trong quá trình xóa sinh viên khỏi lớp học phần"
                };
            }
        }
    }
}