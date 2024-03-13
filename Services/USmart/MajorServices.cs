using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using VinhUni_Educator_API.Configs;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class MajorServices : IMajorServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<OrganizationServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        public MajorServices(ApplicationDBContext context, IConfiguration config, ILogger<OrganizationServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
        }
        public async Task<ActionResponse> SyncMajorAsync()
        {
            var APIBaseURL = _config["VinhUNISmart:API"];
            if (string.IsNullOrEmpty(APIBaseURL))
            {
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Cannot find USmart API base URL in configuration"
                };
            }
            try
            {
                // Check if the last sync action is within 30 minutes
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncMajor);
                if (lastSync != null && lastSync.SyncAt.AddMinutes(30) > DateTime.UtcNow)
                {
                    var remainingTime = (lastSync.SyncAt.AddMinutes(30) - DateTime.UtcNow).Minutes;
                    return new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = $"Đã có ai đó thực hiện đồng bộ gần đây, vui lòng đợi {remainingTime} phút trước khi thực hiện lại"
                    };
                }
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("Cannot find user id in context");
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new Exception("Cannot find user in database");
                }
                var uSmartToken = await _context.USmartTokens.FirstOrDefaultAsync(t => t.UserId == userId);
                var isTokenExpired = _jwtServices.IsTokenExpired(uSmartToken?.Token);
                if (uSmartToken == null || isTokenExpired)
                {
                    return new ActionResponse
                    {
                        StatusCode = 401,
                        IsSuccess = false,
                        Message = "User has not been authorized to use USmart API"
                    };
                }
                var fetch = new FetchData(APIBaseURL, uSmartToken.Token);
                var formBody = @"{
                                ""pageInfo"": {
                                    ""page"": 1,
                                    ""pageSize"": 1000
                                },
                                ""sorts"": [],
                                ""filters"": [
                                    {
                                        ""filters"": [],
                                        ""field"": ""idTrinhDoDaoTao"",
                                        ""operator"": ""eq"",
                                        ""value"": 5 // Đại học
                                    }
                                ],
                                ""fields"": ""id,nganH_Ma,nganH_Ten,nganH_ThoiGianToiThieu,nganH_ThoiGianToiDa""
                                }";
                var responseData = await fetch.FetchAsync("gwsg/dbdaotao_chinhquy/tbl_DM_NguoiHoc_Nganh/getPaged", null, formBody, Method.Post);
                List<MajorSyncModel> listMajor = JsonSerializer.Deserialize<List<MajorSyncModel>>(responseData?.data?.ToString());
                if (responseData?.success == false || listMajor is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Error occurred while getting organizations from USmart API"
                    };
                }
                // Update or insert majors to database
                int countNewMajor = 0;
                foreach (var org in listMajor)
                {
                    var major = await _context.Majors.FirstOrDefaultAsync(o => o.MajorId == org.id);
                    if (major is null)
                    {
                        major = new Major
                        {
                            MajorId = org.id,
                            MajorCode = org.nganH_Ma,
                            MajorName = org.nganH_Ten,
                            MinTrainingYears = org.nganH_ThoiGianToiThieu,
                            MaxTrainingYears = org.nganH_ThoiGianToiDa,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _context.Majors.AddAsync(major);
                        countNewMajor++;
                    }
                }
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncOrganization,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm {countNewMajor} ngành đào tạo mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                _context.SaveChanges();
                var message = countNewMajor > 0 ? $"Đã cập nhật thêm {countNewMajor} đơn vị mới" : "Không có đơn vị nào mới";
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Cập nhật danh sách ngành học thành công",
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while syncing organizations: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Error occurred while syncing organizations, please try again later or contact administrator"
                };
            }
        }
    }
}