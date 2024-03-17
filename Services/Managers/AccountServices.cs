using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        public async Task<ActionResponse> GetCurrentUser(bool skipCache = true)
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
                        Message = "User is not authenticated"
                    };
                }
                var userId = userContext?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");
                if (skipCache)
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
    }
}