
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class AccountServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<MajorServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        public AccountServices(ApplicationDBContext context, IConfiguration config, ILogger<MajorServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
        }
        // public async Task<ActionResponse> GetUserById(int id)
        // {
        //     var APIBaseURL = _config["VinhUNISmart:API"];
        //     if (string.IsNullOrEmpty(APIBaseURL))
        //     {
        //         return new ActionResponse
        //         {
        //             StatusCode = 500,
        //             IsSuccess = false,
        //             Message = "Không thể tìm thấy địa chỉ API của hệ thống USmart trong cấu hình"
        //         };
        //     }
        //     try
        //     {
        //         // Check if the last sync action is within 30 minutes
        //         // var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncMajor);
        //         // if (lastSync != null && lastSync.SyncAt.AddMinutes(30) > DateTime.UtcNow)
        //         // {
        //         //     var remainingTime = (lastSync.SyncAt.AddMinutes(30) - DateTime.UtcNow).Minutes;
        //         //     return new ActionResponse
        //         //     {
        //         //         StatusCode = 400,
        //         //         IsSuccess = false,
        //         //         Message = $"Đã có ai đó thực hiện đồng bộ gần đây, vui lòng đợi {remainingTime} phút trước khi thực hiện lại"
        //         //     };
        //         // }
        //         var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        //         if (string.IsNullOrEmpty(userId))
        //         {
        //             throw new Exception("Không thể xác định người dùng");
        //         }
        //         var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        //         if (user == null)
        //         {
        //             throw new Exception("Không thể xác định người dùng");
        //         }
        //         var uSmartToken = await _context.USmartTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        //         var isTokenExpired = _jwtServices.IsTokenExpired(uSmartToken?.Token);
        //         if (uSmartToken == null || isTokenExpired)
        //         {
        //             return new ActionResponse
        //             {
        //                 StatusCode = 403,
        //                 IsSuccess = false,
        //                 Message = "Bạn không có quyền truy cập vào hệ thống USmart, vui lòng đăng nhập lại bằng tài tài USmart"
        //             };
        //         }
        //         var fetch = new FetchData(APIBaseURL, uSmartToken.Token);
        //         var responseData = await fetch.FetchAsync("gwsg/organizationmanagement/user/14885", Method.Get);
        //         UserSyncModel userInfo = JsonSerializer.Deserialize<UserSyncModel>(responseData?.data?.ToString());
        //         return new ActionResponse
        //         {
        //             StatusCode = 200,
        //             IsSuccess = true,
        //             Message = "Đồng bộ ngành học thành công",
        //             Data = userInfo
        //         };

        //     }
        //     catch (Exception ex)
        //     {
        //         return new ActionResponse
        //         {
        //             StatusCode = 500,
        //             IsSuccess = false,
        //             Message = ex.Message
        //         };
        //     }
        // }
    }
}