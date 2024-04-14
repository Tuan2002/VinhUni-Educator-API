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

namespace VinhUni_Educator_API.Services.Managers
{
    public class TeacherServices : ITeacherServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<TeacherServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public TeacherServices(ApplicationDBContext context, IConfiguration config, ILogger<TeacherServices> logger, IHttpContextAccessor httpContextAccessor, IJwtServices jwtServices, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtServices = jwtServices;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<ActionResponse> GetTeachersAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPage = pageIndex ?? DEFAULT_PAGE_INDEX;
                int pageSize = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.Teachers.AsQueryable();
                query = query.Where(x => x.IsDeleted == false);
                query.OrderByDescending(x => x.CreatedAt);
                var teachers = await PageList<Teacher, TeacherViewModel>.CreateWithMapperAsync(query, currentPage, pageSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách giảng viên thành công",
                    Data = teachers
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting teachers: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi lấy danh sách giảng viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetTeachersByOrganizationAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizations.FirstOrDefaultAsync(x => x.Id == organizationId);
                if (organization == null || organization.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị đã bị xóa",
                    };
                }
                var rawTeachers = await _context.Teachers.Where(x => x.OrganizationId == organizationId && x.IsDeleted == false).ToListAsync();
                var teachers = _mapper.Map<List<TeacherViewModel>>(rawTeachers);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách giảng viên theo đơn vị thành công",
                    Data = teachers
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting teachers by organization: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi lấy danh sách giảng viên theo đơn vị, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetTeacherByIdAsync(int teacherId)
        {
            try
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId);
                if (teacher == null || teacher.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy giảng viên hoặc giảng viên đã bị xóa",
                    };
                }
                var teacherViewModel = _mapper.Map<TeacherViewModel>(teacher);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin giảng viên thành công",
                    Data = teacherViewModel
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting teacher by id: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi lấy thông tin giảng viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetTeacherByCodeAsync(int teacherCode)
        {
            try
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.TeacherCode == teacherCode);
                if (teacher == null || teacher.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy giảng viên hoặc giảng viên đã bị xóa",
                    };
                }
                var teacherViewModel = _mapper.Map<TeacherViewModel>(teacher);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin giảng viên thành công",
                    Data = teacherViewModel
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting teacher by code: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi lấy thông tin giảng viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetDeletedTeachersAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPage = pageIndex ?? DEFAULT_PAGE_INDEX;
                int pageSize = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.Teachers.AsQueryable();
                query = query.Where(x => x.IsDeleted == true);
                query.OrderByDescending(x => x.DeletedAt);
                var teachers = await PageList<Teacher, TeacherViewModel>.CreateWithMapperAsync(query, currentPage, pageSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách giảng viên đã xóa thành công",
                    Data = teachers
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting deleted teachers: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi lấy danh sách giảng viên đã xóa, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> GetImportableTeachersByOrganization(int organizationId)
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
                var organization = await _context.Organizations.FirstOrDefaultAsync(x => x.Id == organizationId);
                if (organization == null || organization.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị đã bị xóa",
                    };
                }
                var fetch = new FetchData(APIBaseURL, smartToken.Token);
                var formBody = @"{
                                ""pageInfo"": {
                                    ""page"": 1,
                                    ""pageSize"": 1000
                                },
                                ""filters"": [
                                    {
                                        ""field"": ""dV_ID_GiangDay"",
                                        ""operator"": ""in"",
                                        ""value"": """ + organization.OrganizationCode + @"""
                                    }      
                                ],
                                ""sorts"": [],
                                }";
                var response = await fetch.FetchAsync("gwsg/dbcanbo/tbl_CANBO_HoSo/getPaged", null, formBody, Method.Post);
                List<TeacherSyncModel> teacherList = JsonSerializer.Deserialize<List<TeacherSyncModel>>(response?.data?.ToString());
                if (response?.success == false || teacherList == null)
                {
                    throw new Exception("Cannot get teachers from USmart");
                }
                var importableTeachers = teacherList.Where(x => !_context.Teachers.Any(y => y.TeacherCode == x.hS_ID)).ToList();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách giảng viên có thể nhập thành công",
                    Data = _mapper.Map<List<ImportTeacherModel>>(importableTeachers)
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting importable teachers: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi lấy danh sách giảng viên có thể nhập, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> ImportTeachersByOrganizationAsync(int organizationId, List<ImportTeacherModel> teachers)
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
                var organization = await _context.Organizations.FirstOrDefaultAsync(x => x.Id == organizationId);
                if (organization == null || organization.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị đã bị xóa",
                    };
                }
                if (teachers == null || teachers.Count == 0)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Danh sách giảng viên trống",
                    };
                }
                int failCount = 0;
                int successCount = 0;
                List<ImportTeacherViewModel> importResults = [];
                await _context.Database.BeginTransactionAsync();
                foreach (var teacher in teachers)
                {
                    var resultItem = _mapper.Map<ImportTeacherViewModel>(teacher);
                    var existTeacher = await _context.Teachers.AnyAsync(x => x.TeacherCode == teacher.TeacherCode);
                    if (existTeacher || teacher.OrganizationCode != organization.OrganizationCode)
                    {
                        resultItem.ErrorMessage = "Giảng viên đã tồn tại hoặc không thuộc đơn vị";
                        importResults.Add(resultItem);
                        failCount++;
                        continue;
                    }
                    var newTeacher = new Teacher
                    {
                        TeacherId = teacher.TeacherId,
                        TeacherCode = teacher.TeacherCode,
                        FirstName = teacher.FirstName,
                        LastName = teacher.LastName,
                        Gender = teacher.Gender,
                        Dob = teacher.Dob,
                        Email = teacher.Email,
                        OrganizationId = organization.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = userId,
                        SmartId = teacher.SSOId,
                        IsSynced = true
                    };
                    await _context.Teachers.AddAsync(newTeacher);
                    resultItem.IsImported = true;
                    resultItem.OrganizationName = organization.OrganizationName;
                    importResults.Add(resultItem);
                    successCount++;
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status201Created,
                    IsSuccess = true,
                    Message = "Nhập danh sách giảng viên thành công",
                    Data = new
                    {
                        successCount,
                        failCount,
                        importResults
                    }
                };
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                _logger.LogError($"Error occurred while importing teachers: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi nhập danh sách giảng viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> DeleteTeacherAsync(int teacherId)
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
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId);
                if (teacher == null || teacher.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy giảng viên hoặc giảng viên đã bị xóa",
                    };
                }
                teacher.IsDeleted = true;
                teacher.DeletedAt = DateTime.UtcNow;
                teacher.DeletedBy = userId;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa giảng viên thành công",
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while deleting teacher: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi xóa giảng viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> RestoreTeacherAsync(int teacherId)
        {
            try
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId);
                if (teacher == null || teacher.IsDeleted == false)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy giảng viên",
                    };
                }
                teacher.IsDeleted = false;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Khôi phục giảng viên thành công",
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while restoring teacher: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi khôi phục giảng viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> UpdateTeacherAsync(int teacherId, UpdateTeacherModel model)
        {
            try
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId);
                if (teacher == null || teacher.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy giảng viên hoặc giảng viên đã bị xóa",
                    };
                }
                var organization = await _context.Organizations.FirstOrDefaultAsync(x => x.Id == model.OrganizationId);
                if (organization == null || organization.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị đã bị xóa",
                    };
                }
                teacher.FirstName = model.FirstName ?? teacher.FirstName;
                teacher.LastName = model.LastName ?? teacher.LastName;
                teacher.Email = model.Email ?? teacher.Email;
                teacher.Dob = model.Dob ?? teacher.Dob;
                teacher.Gender = model.Gender ?? teacher.Gender;
                teacher.OrganizationId = model.OrganizationId ?? teacher.OrganizationId;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật giảng viên thành công",
                    Data = _mapper.Map<TeacherViewModel>(teacher)
                };

            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while updating teacher: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi cập nhật giảng viên, vui lòng thử lại sau",
                };
            }
        }
        public async Task<ActionResponse> SearchTeacherAsync(string? searchKey, int? limit)
        {
            try
            {
                int searchLimit = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _context.Teachers.AsQueryable();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    query = query.Where(x => x.FirstName != null && x.FirstName.Contains(searchKey) || x.LastName.Contains(searchKey) || x.TeacherCode.ToString().Contains(searchKey));
                }
                query.OrderByDescending(x => x.CreatedAt);
                var rawTeachers = await query.Take(searchLimit).ToListAsync();
                var teachers = _mapper.Map<List<TeacherViewModel>>(rawTeachers);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Tìm kiếm giảng viên thành công",
                    Data = teachers
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while searching teachers: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi khi tìm kiếm giảng viên, vui lòng thử lại sau",
                };
            }
        }
    }
}