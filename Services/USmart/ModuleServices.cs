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
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class ModuleServices : IModuleServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<ModuleServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public ModuleServices(ApplicationDBContext context, IConfiguration config, ILogger<ModuleServices> logger, IHttpContextAccessor httpContextAccessor, IJwtServices jwtServices, IMapper mapper)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtServices = jwtServices;
            _mapper = mapper;
        }
        public async Task<ActionResponse> SyncModulesAsync()
        {
            try
            {
                var APIBaseURL = _config["VinhUNISmart:API"];
                if (string.IsNullOrEmpty(APIBaseURL))
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Không thể tìm thấy địa chỉ API của hệ thống USmart trong cấu hình"
                    };
                }
                // Check if there is any recent sync action
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncModule);
                if (lastSync != null && lastSync.SyncAt.AddMinutes(SyncActionList.SYNC_TIME_OUT + 50) > DateTime.UtcNow)
                {
                    var remainingTime = (lastSync.SyncAt.AddMinutes(SyncActionList.SYNC_TIME_OUT + 50) - DateTime.UtcNow).Minutes;
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
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    };
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
                        StatusCode = StatusCodes.Status403Forbidden,
                        IsSuccess = false,
                        Message = "Bạn không có quyền truy cập vào hệ thống USmart, vui lòng đăng nhập lại bằng tài khoản USmart"
                    };
                }
                var fetch = new FetchData(APIBaseURL, uSmartToken.Token);
                var formBody = @"{
                                ""pageInfo"": {
                                    ""page"": 1,
                                    ""pageSize"": 5000
                                },
                                ""sorts"": [
                                {
                                    ""field"": ""id"",
                                    ""dir"": -1
                                }
                                ],
                                ""filters"": [
                                    {
                                        ""filters"": [],
                                        ""field"": ""idHe"",
                                        ""operator"": ""eq"",
                                        ""value"": 1 // Đại học chính quy
                                    }
                                ],
                               ""fields"": ""id,code,ten,soTinChi,namApDung""
                                }";
                var response = await fetch.FetchAsync("gwsg/dbdaotao_chinhquy/tbl_HocPhan/getPaged", null, formBody, Method.Post);
                List<ModuleSyncModel> moduleList = JsonSerializer.Deserialize<List<ModuleSyncModel>>(response?.data?.ToString());
                if (response?.success == false || moduleList is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy danh sách khoá đào tạo từ hệ thống USmart"
                    };
                }
                int newModuleCount = 0;
                await _context.Database.BeginTransactionAsync();
                foreach (var module in moduleList)
                {
                    var existingModule = await _context.Modules.AnyAsync(m => m.ModuleId == module.id || m.ModuleCode == module.code);
                    var schoolYear = await _context.SchoolYears.FirstOrDefaultAsync(s => s.YearCode == module.namApDung);
                    if (!existingModule)
                    {
                        var newModule = new Module
                        {
                            ModuleId = module.id,
                            ModuleCode = module.code,
                            ModuleName = module.ten,
                            CreditHours = module.soTinChi,
                            ApplyYearId = schoolYear?.Id,
                            CreatedAt = DateTime.UtcNow,
                            CreatedById = userId
                        };
                        await _context.Modules.AddAsync(newModule);
                        await _context.SaveChangesAsync();
                        newModuleCount++;
                    }
                }
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncModule,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm học phần mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = $"Đã cập nhật thành công {newModuleCount} học phần mới"
                };
            }

            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred in ModuleServices.SyncModulesAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ danh sách học phần, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetModulesAsync(int? pageIndex, int? limit)
        {
            try
            {
                var page = pageIndex ?? DEFAULT_PAGE_INDEX;
                var size = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.Modules.AsQueryable();
                query = query.Where(m => m.IsDeleted == false);
                query = query.OrderByDescending(m => m.CreatedAt);
                var modules = await PageList<Module, ModuleViewModel>.CreateWithMapperAsync(query, page, size, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách học phần thành công",
                    Data = modules
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ModuleServices.GetModulesAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách học phần, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> GetDeletedModulesAsync(int? pageIndex, int? limit)
        {
            try
            {
                var page = pageIndex ?? DEFAULT_PAGE_INDEX;
                var size = limit ?? DEFAULT_PAGE_SIZE;
                var query = _context.Modules.AsQueryable();
                query = query.Where(m => m.IsDeleted == true);
                query = query.OrderByDescending(m => m.DeletedAt);
                var modules = await PageList<Module, ModuleViewModel>.CreateWithMapperAsync(query, page, size, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy danh sách học phần đã xóa thành công",
                    Data = modules
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ModuleServices.GetDeletedModulesAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách học phần đã xóa, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> GetModuleByIdAsync(int moduleId)
        {
            try
            {
                var rawModule = await _context.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
                if (rawModule == null || rawModule.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học phần này hoặc học phần đã bị xóa"
                    };
                }
                var module = _mapper.Map<ModuleViewModel>(rawModule);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin học phần thành công",
                    Data = module
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ModuleServices.GetModuleByIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin học phần, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> GetModuleByCodeAsync(string moduleCode)
        {
            try
            {
                var rawModule = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleCode == moduleCode);
                if (rawModule == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học phần này"
                    };
                }
                var module = _mapper.Map<ModuleViewModel>(rawModule);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy thông tin học phần thành công",
                    Data = module
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ModuleServices.GetModuleByCodeAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin học phần, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> DeleteModuleAsync(int moduleId)
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
                var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
                if (module == null || module.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học phần hoặc học phần đã bị xóa"
                    };
                }
                module.IsDeleted = true;
                module.DeletedAt = DateTime.UtcNow;
                module.DeletedBy = userId;
                _context.Modules.Update(module);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa học phần thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ModuleServices.DeleteModuleAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa học phần, vui lòng thử lại sau"
                };
            }
        }

        public async Task<ActionResponse> RestoreModuleAsync(int moduleId)
        {
            try
            {
                var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
                if (module == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học phần"
                    };
                }
                module.IsDeleted = false;
                _context.Modules.Update(module);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Khôi phục học phần thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ModuleServices.RestoreModuleAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục học phần, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> SearchModulesAsync(string? keyword, int? limit)
        {
            try
            {
                var size = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _context.Modules.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {

                    query = query.Where(m => m.ModuleName.Contains(keyword) || m.ModuleCode.Contains(keyword));
                }
                query = query.Where(m => m.IsDeleted == false);
                query = query.OrderByDescending(m => m.CreatedAt);
                var rawModules = await query.Take(size).ToListAsync();
                var modules = _mapper.Map<List<ModuleViewModel>>(rawModules);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Tìm kiếm học phần thành công",
                    Data = modules
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in ModuleServices.SearchModulesAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tìm kiếm học phần, vui lòng thử lại sau"
                };
            }
        }
    }
}