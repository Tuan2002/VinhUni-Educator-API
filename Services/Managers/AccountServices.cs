using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class AccountServices : IAccountServices
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<MajorServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheServices _cacheServices;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountServices(ApplicationDBContext context, ILogger<MajorServices> logger, IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager, ICacheServices cacheServices, IMapper mapper)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _cacheServices = cacheServices;
            _mapper = mapper;
        }

        public async Task<ActionResponse> ChangePasswordAsync(ChangePasswordModel model)
        {
            if (!model.CheckSamePassword())
            {
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    IsSuccess = false,
                    Message = "Mật khẩu và xác nhận mật khẩu không trùng khớp"
                };
            }
            try
            {
                var userContext = _httpContextAccessor?.HttpContext?.User;
                if (!userContext?.Identity?.IsAuthenticated ?? false)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn chưa đăng nhập vào hệ thống"
                    };
                }
                if (!model.CheckSameOldPassword())
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Mật khẩu mới không được trùng với mật khẩu cũ"
                    };
                }
                var currentUser = userContext != null ? await _userManager.GetUserAsync(userContext) : throw new Exception("Không tìm thấy thông tin người dùng");
                if (currentUser == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                var checkPassword = await _userManager.CheckPasswordAsync(currentUser, model.OldPassword);
                if (!checkPassword)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Mật khẩu đã nhập không chính xác"
                    };
                }
                var response = await _userManager.ChangePasswordAsync(currentUser, model.OldPassword, model.NewPassword);
                if (!response.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Thay đổi mật khẩu không thành công"
                    };
                }
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Thay đổi mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/ChangePasswordAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> FirstChangePasswordAsync(ResetPasswordModel model)
        {
            if (!model.CheckSamePassword())
            {
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    IsSuccess = false,
                    Message = "Mật khẩu và xác nhận mật khẩu không trùng khớp"
                };
            }
            try
            {
                var userContext = _httpContextAccessor?.HttpContext?.User;
                if (!userContext?.Identity?.IsAuthenticated ?? false)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn chưa đăng nhập vào hệ thống"
                    };
                }
                var currentUser = userContext != null ? await _userManager.GetUserAsync(userContext) : throw new Exception("Không tìm thấy thông tin người dùng");
                if (currentUser == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                if (currentUser.IsPasswordChanged)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Mật khẩu của bạn đã được cập nhật trước đó, vui lòng sử dụng chức năng quên mật khẩu để cập nhật mật khẩu mới"
                    };
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
                var response = await _userManager.ResetPasswordAsync(currentUser, token, model.NewPassword);
                if (!response.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Thay đổi mật khẩu không thành công"
                    };
                }
                currentUser.IsPasswordChanged = true;
                await _userManager.UpdateAsync(currentUser);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Thay đổi mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/FirstChangePasswordAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                var otp = new Random().Next(100000, 999999).ToString();
                OTPVerifyModel OTPVerifyModel = new OTPVerifyModel
                {
                    OTP = otp,
                    UserId = user.Id
                };
                // Send OTP to user's email
                // I will not include the code to send email here
                await _cacheServices.SetDataAsync<OTPVerifyModel>($"FORGOT-PASSWORD-ID:{OTPVerifyModel.Id}", OTPVerifyModel, new DateTimeOffset(DateTime.UtcNow.AddMinutes(5)));
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("FORGOT-PASSWORD-ID", OTPVerifyModel.Id, options: new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax

                });
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Mã OTP đã được gửi đến email của bạn"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/ForgotPasswordAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> VerifyOTPAsync(string otp)
        {
            try
            {
                if (string.IsNullOrEmpty(otp))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Vui lòng nhập mã OTP"
                    };
                }
                var requestId = _httpContextAccessor?.HttpContext?.Request.Cookies["FORGOT-PASSWORD-ID"];
                if (string.IsNullOrEmpty(requestId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Mã OTP không hợp lệ hoặc đã hết hạn"
                    };
                }
                var OTPVerifyModel = await _cacheServices.GetDataAsync<OTPVerifyModel>($"FORGOT-PASSWORD-ID:{requestId}");
                if (OTPVerifyModel == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Mã OTP không hợp lệ hoặc đã hết hạn"
                    };
                }
                if (OTPVerifyModel.OTP != otp)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Mã OTP không hợp lệ hoặc đã hết hạn"
                    };
                }
                var user = await _userManager.FindByIdAsync(OTPVerifyModel.UserId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _cacheServices.RemoveDataAsync($"FORGOT-PASSWORD-ID:{requestId}");
                var CookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.None
                };
                _httpContextAccessor?.HttpContext?.Response.Cookies.Delete("FORGOT-PASSWORD-ID");
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("FORGOT-PASSWORD-TOKEN", token, CookieOptions);
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("FORGOT-PASSWORD-UID", user.Id, CookieOptions);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Xác thực OTP thành công",
                    IsSuccess = true,
                };

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/VerifyForgotPasswordOTP: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi xác thực OTP, vui lòng thử lại sau"
                };

            }
        }
        public async Task<ActionResponse> ResetPasswordAsync(ResetPasswordModel model)
        {
            if (!model.CheckSamePassword())
            {
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    IsSuccess = false,
                    Message = "Mật khẩu và xác nhận mật khẩu không trùng khớp"
                };
            }
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.Request.Cookies["FORGOT-PASSWORD-UID"];
                var token = _httpContextAccessor?.HttpContext?.Request.Cookies["FORGOT-PASSWORD-TOKEN"];
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin xác thực mật khẩu"
                    };
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                var response = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!response.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Thay đổi mật khẩu không thành công"
                    };
                }
                _httpContextAccessor?.HttpContext?.Response.Cookies.Delete("FORGOT-PASSWORD-TOKEN");
                _httpContextAccessor?.HttpContext?.Response.Cookies.Delete("FORGOT-PASSWORD-UID");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Thay đổi mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/ResetPasswordAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetCurrentUserAsync(bool? skipCache = false)
        {
            try
            {
                var userContext = _httpContextAccessor?.HttpContext?.User;
                if (!userContext?.Identity?.IsAuthenticated ?? false || userContext == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn chưa đăng nhập vào hệ thống"
                    };
                }
                var userId = userContext?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");
                if (!skipCache ?? false)
                {
                    var currentUserInfo = await _cacheServices.GetDataAsync<PublicUserModel>($"USER-INFO_{userId}");
                    if (currentUserInfo == null)
                    {
                        goto getUserFromDatabase;
                    }
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        IsSuccess = true,
                        Data = currentUserInfo
                    };
                }
            getUserFromDatabase:
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                var userInfo = _mapper.Map<PublicUserModel>(currentUser);
                userInfo.Roles = userRoles;
                await _cacheServices.SetDataAsync($"USER_INFO:{userId}", userInfo, new DateTimeOffset(DateTime.UtcNow.AddMinutes(60)));
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/GetCurrentUser: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> UpdateProfileAsync(UpdateProfileModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor?.HttpContext?.User ?? throw new Exception("Không tìm thấy thông tin người dùng"));
                if (currentUser == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                currentUser.FirstName = model.FirstName ?? currentUser.FirstName;
                currentUser.LastName = model.LastName ?? currentUser.LastName;
                currentUser.Email = model.Email ?? currentUser.Email;
                currentUser.Gender = model.Gender ?? currentUser.Gender;
                currentUser.PhoneNumber = model.PhoneNumber ?? currentUser.PhoneNumber;
                currentUser.Address = model.Address ?? currentUser.Address;
                currentUser.DateOfBirth = model.DateOfBirth ?? currentUser.DateOfBirth;
                var result = await _userManager.UpdateAsync(currentUser);
                if (!result.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Cập nhật thông tin người dùng không thành công"
                    };
                }
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                var newUserInfo = _mapper.Map<PublicUserModel>(currentUser);
                newUserInfo.Roles = userRoles;
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin người dùng thành công",
                    Data = newUserInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/UpdateProfileAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> UploadProfileImageAsync(UploadProfileImage image)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor?.HttpContext?.User ?? throw new Exception("Không tìm thấy thông tin người dùng"));
                if (currentUser == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    };
                }
                currentUser.Avatar = image.ImageURL;
                var result = await _userManager.UpdateAsync(currentUser);
                if (!result.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "Cập nhật ảnh đại diện không thành công"
                    };
                }
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Cập nhật ảnh đại diện thành công",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/UpdateProfileImage: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ActionResponse> GetStudentProfileAsync()
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
                        Message = "Bạn chưa đăng nhập vào hệ thống"
                    };
                }
                var rawStudent = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId);
                if (rawStudent == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    };
                }
                var student = _mapper.Map<StudentViewModel>(rawStudent);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Data = student
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/GetStudentProfileAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin sinh viên"
                };
            }
        }
        public async Task<ActionResponse> GetTeacherProfileAsync()
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
                        Message = "Bạn chưa đăng nhập vào hệ thống"
                    };
                }
                var rawTeacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId);
                if (rawTeacher == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy thông tin giảng viên"
                    };
                }
                var teacher = _mapper.Map<TeacherViewModel>(rawTeacher);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Data = teacher
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AccountServices/GetTeacherProfileAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin giảng viên"
                };
            }
        }
    }
}