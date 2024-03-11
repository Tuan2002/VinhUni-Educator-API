using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Authenticators;
using VinhUni_Educator_API.Configs;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models.USmart;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class OrganizationServices : IOrganizationServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<OrganizationServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        public OrganizationServices(ApplicationDBContext context, IConfiguration config, ILogger<OrganizationServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
        }

        public async Task<ActionResponse> SyncOrganizationsAsync()
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
            {// Check if the last sync action is within 30 minutes
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncOrganization);
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
                var authenticator = new JwtAuthenticator(uSmartToken.Token);
                var options = new RestClientOptions(APIBaseURL)
                {
                    Authenticator = authenticator
                };
                var client = new RestClient(options);
                var request = new RestRequest("gwsg/organizationmanagement/Organization/GetByParentCode", Method.Get);
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new ActionResponse
                    {
                        StatusCode = (int)response.StatusCode,
                        IsSuccess = false,
                        Message = "Error occurred while getting organizations from USmart API"
                    };
                }
                if (string.IsNullOrEmpty(response.Content))
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Cannot get organizations from USmart API"
                    };
                }
                var responseData = JsonSerializer.Deserialize<APIResponse>(response.Content);
                List<OrganizationSyncModel> listOrganization = JsonSerializer.Deserialize<List<OrganizationSyncModel>>(responseData?.data?.ToString());
                if (responseData?.success == false || listOrganization is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Error occurred while getting organizations from USmart API"
                    };
                }
                // Update or insert organizations to database
                int countNewOrganization = 0;
                foreach (var org in listOrganization)
                {
                    var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationCode == org.id);
                    if (organization is null)
                    {
                        organization = new Organization
                        {
                            OrganizationCode = org.id,
                            OrganizationName = org.name,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _context.Organizations.AddAsync(organization);
                        countNewOrganization++;
                    }
                }
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncOrganization,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm {countNewOrganization} đơn vị mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                _context.SaveChanges();
                var message = countNewOrganization > 0 ? $"Đã cập nhật thêm {countNewOrganization} đơn vị mới" : "Không có đơn vị nào mới";
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = message,
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