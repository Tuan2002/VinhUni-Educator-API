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
    public class UserServices : IUserServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtServices _jwtServices;
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly ILogger<UserServices> _logger;
        public int DEFAULT_PAGE_SIZE = 10;
        public int DEFAULT_PAGE_INDEX = 1;
        public int DEFAULT_SEARCH_RESULT = 10;
        public UserServices(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDBContext context, IJwtServices jwtServices, IConfiguration config, IMapper mapper, ILogger<UserServices> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
            _jwtServices = jwtServices;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ActionResponse> SyncUserFromSSO(string token)
        {
            var APIBaseURL = _config["VinhUNISmart:API"];
            if (token == null || string.IsNullOrEmpty(APIBaseURL))
            {
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    IsSuccess = false,
                    Message = "Không thể xác minh thông tin người dùng"
                };
            }
            try
            {
                List<Claim> claims = _jwtServices.GetTokenClaims(token);
                if (claims == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không thể xác minh thông tin người dùng"
                    };
                }
                await _context.Database.BeginTransactionAsync();
                // Get user info from SSO Vinh Uni
                string? smartUserId = claims?.FirstOrDefault(c => c.Type == "userid")?.Value.ToString();
                var fetch = new FetchData(APIBaseURL, token);
                var response = await fetch.FetchAsync($"gwsg/organizationmanagement/user/{smartUserId}", Method.Get);
                if (response?.success == false)
                {
                    throw new Exception("Không thể lấy thông tin người dùng từ máy chủ");
                }
                UserSyncModel userSync = JsonSerializer.Deserialize<UserSyncModel>(response?.data?.ToString()) ?? throw new Exception("Lỗi máy chủ, vui lòng thử lại sau");
                // Create user in local database
                string userRole = userSync.source == "2" ? AppRoles.Teacher : AppRoles.Student;
                var user = new ApplicationUser
                {
                    UserName = userSync.userName,
                    USmartId = userSync.id,
                    Email = userSync.email,
                    FirstName = userSync.firstName,
                    LastName = userSync.lastName,
                    PhoneNumber = userSync.phoneNumber,
                    Gender = userSync.gender,
                    DateOfBirth = DateOnly.FromDateTime(userSync.dob),
                    CreatedAt = DateTime.UtcNow,
                    IsPasswordChanged = false
                };
                var createResponse = await _userManager.CreateAsync(user, userSync.GeneratePassword());
                if (!createResponse.Succeeded)
                {
                    await _context.Database.RollbackTransactionAsync();
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Không thể tạo tài khoản người dùng"
                    };
                }
                // Add user to role
                if (!await _roleManager.RoleExistsAsync(userRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(userRole));
                }
                await _userManager.AddToRoleAsync(user, userRole);
                // Sync user info to local database
                switch (userSync.source)
                {
                    case "1":
                        // Student info
                        var StudentCode = claims?.FirstOrDefault(c => c.Type == "maNguoiHoc")?.Value.ToString();
                        if (string.IsNullOrEmpty(StudentCode))
                        {
                            await _context.Database.RollbackTransactionAsync();
                            return new ActionResponse
                            {
                                StatusCode = StatusCodes.Status404NotFound,
                                IsSuccess = false,
                                Message = "Không thể xác minh thông tin người học"
                            };
                        }
                        // Get student info from SSO Vinh Uni
                        var fetchStudentData = await fetch.FetchAsync($"gwsg/dbnguoihoc/tbl_NguoiHoc_HoSo/GetByCode/{StudentCode}", Method.Get);
                        if (fetchStudentData?.success == false)
                        {
                            throw new Exception("Không thể lấy thông tin người học từ máy chủ");
                        }
                        // Deserialize student info
                        StudentSyncModel studentData = JsonSerializer.Deserialize<StudentSyncModel>(fetchStudentData?.data?.ToString()) ?? throw new Exception("Không nhận dạng được dữ liệu người học");
                        // Get course, program, primary class
                        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == studentData.idKhoaHoc);
                        var program = await _context.TrainingPrograms.FirstOrDefaultAsync(p => p.ProgramCode == studentData.idNganh);
                        var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(c => c.ClassId == studentData.idLopHanhChinh);
                        // Check if course, program, primary class exists
                        if (course is null || program is null || primaryClass is null)
                        {
                            await _context.Database.RollbackTransactionAsync();
                            return new ActionResponse
                            {
                                StatusCode = StatusCodes.Status404NotFound,
                                IsSuccess = false,
                                Message = "Không thể xác minh thông tin người học"
                            };
                        }
                        // Check if student exists
                        var student = _context.Students.FirstOrDefault(s => s.StudentCode == studentData.code);
                        // Create or update student info
                        if (student == null)
                        {
                            student = new Student
                            {
                                StudentId = studentData.id,
                                StudentCode = studentData.code,
                                FirstName = studentData.ho,
                                LastName = studentData.ten,
                                Gender = studentData.gioiTinh,
                                Dob = DateOnly.FromDateTime(studentData.ngaySinh),
                                UserId = user.Id,
                                ClassId = primaryClass.Id,
                                ProgramId = program.Id,
                                CourseId = course.Id,
                                CreatedAt = DateTime.UtcNow,
                                IsSynced = true,
                                CreatedById = user.Id
                            };
                            await _context.Students.AddAsync(student);
                        }
                        else
                        {
                            student.FirstName = studentData.ho;
                            student.LastName = studentData.ten;
                            student.Gender = studentData.gioiTinh;
                            student.Dob = DateOnly.FromDateTime(studentData.ngaySinh);
                            student.UserId = user.Id;
                            student.ClassId = primaryClass.Id;
                            student.ProgramId = program.Id;
                            student.CourseId = course.Id;
                            student.IsSynced = true;
                            _context.Students.Update(student);
                        }
                        break;
                    case "2":
                        // Teacher info
                        var teacherCode = claims?.FirstOrDefault(c => c.Type == "maCanBo")?.Value.ToString();
                        if (string.IsNullOrEmpty(teacherCode))
                        {
                            await _context.Database.RollbackTransactionAsync();
                            return new ActionResponse
                            {
                                StatusCode = StatusCodes.Status404NotFound,
                                IsSuccess = false,
                                Message = "Không thể xác minh thông tin giáo viên"
                            };
                        }
                        // Get teacher info from SSO Vinh Uni
                        var fetchTeacherData = await fetch.FetchAsync($"gwsg/dbcanbo/tbl_CANBO_HoSo/GetByCode/{teacherCode}", Method.Get);
                        if (fetchTeacherData?.success == false)
                        {
                            throw new Exception("Không thể lấy thông tin giáo viên từ máy chủ");
                        }
                        // Deserialize teacher info
                        TeacherSyncModel teacherData = JsonSerializer.Deserialize<TeacherSyncModel>(fetchTeacherData?.data?.ToString()) ?? throw new Exception("Không nhận dạng được dữ liệu giáo viên");
                        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationCode == teacherData.dV_ID_GiangDay);
                        // Check if teacher exists
                        var teacher = _context.Teachers.FirstOrDefault(t => t.TeacherCode == teacherData.hS_ID);
                        // Create or update teacher info
                        if (teacher == null)
                        {
                            teacher = new Teacher
                            {
                                TeacherId = teacherData.id,
                                TeacherCode = teacherData.hS_ID,
                                FirstName = teacherData.hS_Ho,
                                LastName = teacherData.hS_Ten,
                                Gender = teacherData.hS_GioiTinh,
                                Email = teacherData.hS_Email ?? user.Email,
                                Dob = DateOnly.FromDateTime(teacherData.ngaySinh),
                                UserId = user.Id,
                                OrganizationId = organization?.Id,
                                CreatedAt = DateTime.UtcNow,
                                IsSynced = true,
                                CreatedById = user.Id
                            };
                            await _context.Teachers.AddAsync(teacher);
                        }
                        else
                        {
                            teacher.FirstName = teacherData.hS_Ho;
                            teacher.LastName = teacherData.hS_Ten;
                            teacher.Gender = teacherData.hS_GioiTinh;
                            teacher.Email = teacherData.hS_Email ?? user.Email;
                            teacher.Dob = DateOnly.FromDateTime(teacherData.ngaySinh);
                            teacher.UserId = user.Id;
                            teacher.OrganizationId = organization?.Id;
                            teacher.IsSynced = true;
                            _context.Teachers.Update(teacher);
                        }
                        break;
                    // Sync teacher info to local database
                    default:
                        throw new Exception("Không thể xác minh thông tin người dùng");
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Tạo tài khoản người dùng thành công",
                    Data = user
                };
            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                _logger.LogError($"Error in UserService/SyncFromSSO: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> CreateUserAsync(CreateUserModel model)
        {
            try
            {
                var newUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    USmartId = model.USmartId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    Avatar = model.Avatar,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                };
                var result = await _userManager.CreateAsync(newUser, model.GeneratePassword());
                if (!result.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Không thể tạo tài khoản người dùng"
                    };
                }
                if (model.Roles != null)
                {
                    foreach (var role in model.Roles)
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            continue;
                        }
                        await _userManager.AddToRoleAsync(newUser, role);
                    }
                }
                var currentUser = _mapper.Map<UserViewModel>(newUser);
                currentUser.Roles = model.Roles;
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Tạo tài khoản người dùng thành công",
                    Data = currentUser
                };

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/CreateUserAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse> GetUsersAsync(int? PageIndex, int? limit)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();
                query = query.Where(u => u.IsDeleted == false || u.IsDeleted == null);
                var pageIndex = PageIndex ?? DEFAULT_PAGE_INDEX;
                var pageSize = limit ?? DEFAULT_PAGE_SIZE;
                // Get users with pagination
                var result = await PageList<ApplicationUser>.CreateAsync(query, pageIndex, pageSize);
                var userList = new PageList<UserViewModel>(_mapper.Map<List<UserViewModel>>(result.Items), result.TotalCount, result.PageIndex, result.PageSize);
                foreach (var user in result.Items)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var userVM = userList.Items.FirstOrDefault(u => u.Id == user.Id);
                    if (userVM != null)
                    {
                        userVM.Roles = roles;
                    }
                }
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy danh sách người dùng thành công",
                    Data = userList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/GetUsersAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> GetDeletedUsersAsync(int? PageIndex, int? limit)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();
                query = query.Where(u => u.IsDeleted == true);
                var pageIndex = PageIndex ?? DEFAULT_PAGE_INDEX;
                var pageSize = limit ?? DEFAULT_PAGE_SIZE;
                // Get deleted users with pagination
                var result = await PageList<ApplicationUser>.CreateAsync(query, pageIndex, pageSize);
                var userList = new PageList<UserViewModel>(_mapper.Map<List<UserViewModel>>(result.Items), result.TotalCount, result.PageIndex, result.PageSize);
                foreach (var user in result.Items)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var userVM = userList.Items.FirstOrDefault(u => u.Id == user.Id);
                    if (userVM != null)
                    {
                        userVM.Roles = roles;
                    }
                }
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy danh sách người dùng đã xóa thành công",
                    Data = userList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/GetDeletedUsers: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };

            }
        }
        public async Task<ActionResponse> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.DeletedBy = userId;
                await _userManager.UpdateAsync(user);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Xóa người dùng thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/DeleteUserAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> RestoreUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }
                user.IsDeleted = false;
                user.DeletedAt = null;
                user.DeletedBy = null;
                await _userManager.UpdateAsync(user);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Khôi phục người dùng thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/RestoreUserAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }
                var roles = await _userManager.GetRolesAsync(user);
                var userInfo = _mapper.Map<UserViewModel>(user);
                userInfo.Roles = roles;
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy thông tin người dùng thành công",
                    Data = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/GetUserByIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> GetUserByNameAsync(string userName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }
                var roles = await _userManager.GetRolesAsync(user);
                var userInfo = _mapper.Map<UserViewModel>(user);
                userInfo.Roles = roles;
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Lấy thông tin người dùng thành công",
                    Data = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/GetUserByNameAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> UpdateUserAsync(string userId, UpdateProfileModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy người dùng"
                    };
                }
                user.FirstName = model.FirstName ?? user.FirstName;
                user.LastName = model.LastName ?? user.LastName;
                user.Email = model.Email ?? user.Email;
                user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
                user.Address = model.Address ?? user.Address;
                user.DateOfBirth = model.DateOfBirth ?? user.DateOfBirth;
                var response = await _userManager.UpdateAsync(user);
                if (!response.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Không thể cập nhật thông tin người dùng"
                    };
                }
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin người dùng thành công",
                    Data = _mapper.Map<UserViewModel>(user)
                };

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/UpdateUserAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> SearchUsersAsync(string searchKey, int? limit)
        {
            try
            {
                var searchResultCount = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _userManager.Users.AsQueryable();
                if (searchKey != null)
                {
                    query = query.Where(u => u.FirstName != null && u.FirstName.Contains(searchKey) || u.LastName != null && u.LastName.Contains(searchKey) || u.UserName != null && u.UserName.Contains(searchKey));
                }
                query = query.Take(searchResultCount);
                var result = await query.ToListAsync();
                var totalCount = result.Count;
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = $"Tìm thấy {totalCount} kết quả",
                    Data = new
                    {
                        TotalCount = totalCount,
                        Users = _mapper.Map<List<UserViewModel>>(result)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserService/SearchUsersAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}