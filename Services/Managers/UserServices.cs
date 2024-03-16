using System.Security.Claims;
using System.Text.Json;
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
        public UserServices(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDBContext context, IJwtServices jwtServices, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
            _jwtServices = jwtServices;
        }
        public async Task<ActionResponse> SyncUserFromSSO(string token)
        {
            var APIBaseURL = _config["VinhUNISmart:API"];
            if (token == null || string.IsNullOrEmpty(APIBaseURL))
            {
                return new ActionResponse
                {
                    StatusCode = 404,
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
                        StatusCode = 404,
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
                        StatusCode = 400,
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
                                StatusCode = 404,
                                IsSuccess = false,
                                Message = "Không thể xác minh thông tin người dùng"
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
                                StatusCode = 404,
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
                    default:
                        break;
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