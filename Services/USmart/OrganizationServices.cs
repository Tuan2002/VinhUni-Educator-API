using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using VinhUni_Educator_API.Configs;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
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
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public OrganizationServices(ApplicationDBContext context, IConfiguration config, ILogger<OrganizationServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices, IMapper mapper)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
            _mapper = mapper;
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
                    Message = "Không thể tìm thấy địa chỉ API của hệ thống USmart trong cấu hình"
                };
            }
            try
            {
                // Check if the last sync action is within 30 minutes
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncOrganization);
                if (lastSync != null && lastSync.SyncAt.AddMinutes(SyncActionList.SYNC_TIME_OUT) > DateTime.UtcNow)
                {
                    var remainingTime = (lastSync.SyncAt.AddMinutes(SyncActionList.SYNC_TIME_OUT) - DateTime.UtcNow).Minutes;
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
                    throw new Exception("Không thể xác định người dùng");
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new Exception("Không thể xác định người dùng");
                }
                var uSmartToken = await _context.USmartTokens.FirstOrDefaultAsync(t => t.UserId == userId);
                var isTokenExpired = _jwtServices.IsTokenExpired(uSmartToken?.Token);
                if (uSmartToken == null || isTokenExpired)
                {
                    return new ActionResponse
                    {
                        StatusCode = 403,
                        IsSuccess = false,
                        Message = "Bạn không có quyền truy cập hệ thống USmart, vui lòng đăng nhập lại bằng tài khoản USmart"
                    };
                }
                var fetch = new FetchData(APIBaseURL, uSmartToken.Token);
                var responseData = await fetch.FetchAsync("gwsg/organizationmanagement/Organization/GetByParentCode", Method.Get);
                List<OrganizationSyncModel> listOrganization = JsonSerializer.Deserialize<List<OrganizationSyncModel>>(responseData?.data?.ToString());
                if (responseData?.success == false || listOrganization is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy danh sách đơn vị từ hệ thống USmart"
                    };
                }
                // Update or insert organizations to database
                int countNewOrganization = 0;
                foreach (var org in listOrganization)
                {
                    var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationId == org.id);
                    if (organization is null)
                    {
                        organization = new Organization
                        {
                            OrganizationId = org.id,
                            OrganizationCode = org.code,
                            OrganizationName = org.name,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _context.Organizations.AddAsync(organization);
                        countNewOrganization++;
                    }
                    else
                    {
                        organization.OrganizationCode = org.code;
                        organization.OrganizationName = org.name;
                        _context.Organizations.Update(organization);
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
                    Message = "Có lỗi xảy ra khi đồng bộ danh sách đơn vị từ hệ thống USmart, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetOrganizationsAsync(int? pageIndex, int? limit)
        {
            try
            {
                int pageSize = limit ?? DEFAULT_PAGE_SIZE;
                int pageNumber = pageIndex ?? DEFAULT_PAGE_INDEX;
                var query = _context.Organizations.AsQueryable();
                query = query.Where(o => o.IsDeleted == false);
                query = query.OrderBy(o => o.CreatedAt);
                var organizationList = await PageList<Organization, OrganizationViewModel>.CreateWithMapperAsync(query, pageNumber, pageSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = organizationList,
                    Message = "Lấy danh sách đơn vị thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting organizations: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách đơn vị, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetDeletedOrganizationsAsync(int? pageIndex, int? limit)
        {
            try
            {
                int pageSize = limit ?? DEFAULT_PAGE_SIZE;
                int pageNumber = pageIndex ?? DEFAULT_PAGE_INDEX;
                var query = _context.Organizations.AsQueryable();
                query = query.Where(o => o.IsDeleted == true);
                query = query.OrderByDescending(o => o.DeletedAt);
                var organizationList = await PageList<Organization, OrganizationViewModel>.CreateWithMapperAsync(query, pageNumber, pageSize, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = organizationList,
                    Message = "Lấy danh sách đơn vị đã xóa thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting deleted organizations: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách đơn vị đã xóa, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetOrganizationByIdAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
                if (organization is null || organization.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị đã bị xóa"
                    };
                }
                var organizationViewModel = _mapper.Map<OrganizationViewModel>(organization);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = organizationViewModel,
                    Message = "Lấy thông tin đơn vị thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting organization by id: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin đơn vị, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> DeleteOrganizationAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
                if (organization is null || organization.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị đã bị xóa"
                    };
                }
                organization.IsDeleted = true;
                organization.DeletedBy = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                organization.DeletedAt = DateTime.UtcNow;
                _context.Organizations.Update(organization);
                _context.SaveChanges();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Xóa đơn vị thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while deleting organization: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa đơn vị, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> RestoreOrganizationAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
                if (organization is null || organization.IsDeleted == false)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị chưa bị xóa"
                    };
                }
                organization.IsDeleted = false;
                _context.Organizations.Update(organization);
                _context.SaveChanges();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Khôi phục đơn vị thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while restoring organization: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục đơn vị, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> UpdateOrganizationAsync(int organizationId, UpdateOrganizationModel model)
        {
            try
            {
                var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
                var existsOrganizationCode = await _context.Organizations.AnyAsync(o => o.OrganizationCode == model.OrganizationCode && o.Id != organizationId);
                if (organization is null || organization.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy đơn vị hoặc đơn vị đã bị xóa"
                    };
                }
                if (existsOrganizationCode)
                {
                    return new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Mã đơn vị đã tồn tại"
                    };
                }
                organization.OrganizationCode = model.OrganizationCode ?? organization.OrganizationCode;
                organization.OrganizationName = model.OrganizationName ?? organization.OrganizationName;
                _context.Organizations.Update(organization);
                _context.SaveChanges();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin đơn vị thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while updating organization: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật thông tin đơn vị, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> SearchOrganizationsAsync(string? searchKey, int? limit)
        {
            try
            {
                int searchLimit = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _context.Organizations.AsQueryable();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    query = query.Where(o => o.OrganizationCode.Contains(searchKey) || o.OrganizationName != null && o.OrganizationName.Contains(searchKey));
                }
                query = query.Where(o => o.IsDeleted == false);
                query = query.OrderBy(o => o.CreatedAt);
                var response = await query.Take(searchLimit).ToListAsync();
                var organizationList = _mapper.Map<List<OrganizationViewModel>>(response);
                int totalOrganizationFound = organizationList.Count;
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = organizationList,
                    Message = $"Tìm thấy {totalOrganizationFound} đơn vị"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while searching organizations: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tìm kiếm đơn vị, vui lòng thử lại sau"
                };
            }
        }

    }
}