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
    public class MajorServices : IMajorServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<MajorServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IJwtServices _jwtServices;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public MajorServices(ApplicationDBContext context, IConfiguration config, ILogger<MajorServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices, IMapper mapper)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
            _mapper = mapper;
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
                    Message = "Không thể tìm thấy địa chỉ API của hệ thống USmart trong cấu hình"
                };
            }
            try
            {
                // Check if the last sync action is within 30 minutes
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncMajor);
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
                        Message = "Bạn không có quyền truy cập vào hệ thống USmart, vui lòng đăng nhập lại bằng tài tài USmart"
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
                        Message = "Có lỗi xảy ra khi lấy danh sách ngành học từ hệ thống USmart"
                    };
                }
                // Update or insert majors to database
                int countNewMajor = 0;
                foreach (var item in listMajor)
                {
                    var major = await _context.Majors.FirstOrDefaultAsync(o => o.MajorId == item.id);
                    if (major is null)
                    {
                        major = new Major
                        {
                            MajorId = item.id,
                            MajorCode = item.nganH_Ma,
                            MajorName = item.nganH_Ten,
                            MinTrainingYears = item.nganH_ThoiGianToiThieu,
                            MaxTrainingYears = item.nganH_ThoiGianToiDa,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _context.Majors.AddAsync(major);
                        countNewMajor++;
                    }
                    else
                    {
                        major.MajorCode = item.nganH_Ma;
                        major.MajorName = item.nganH_Ten;
                        major.MinTrainingYears = item.nganH_ThoiGianToiThieu;
                        major.MaxTrainingYears = item.nganH_ThoiGianToiDa;
                        _context.Majors.Update(major);
                    }
                }
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncMajor,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm {countNewMajor} ngành học mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                await _context.SaveChangesAsync();
                var message = countNewMajor > 0 ? $"Đã cập nhật thêm {countNewMajor} ngành học mới mới" : "Không có ngành học nào mới";
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = message
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while syncing majors: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ danh sách ngành học từ hệ thống USmart, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetMajorsAsync(int? pageIndex, int? limit)
        {
            try
            {
                var query = _context.Majors.AsQueryable();
                query = query.Where(m => m.IsDeleted == false);
                query = query.OrderByDescending(m => m.CreatedAt);
                var currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                var currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var majorList = await PageList<Major, MajorViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = majorList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting majors: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách ngành học, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetDeletedMajorsAsync(int? pageIndex, int? limit)
        {
            try
            {
                var query = _context.Majors.AsQueryable();
                query = query.Where(m => m.IsDeleted == true);
                query = query.OrderByDescending(m => m.DeletedAt);
                var currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                var currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var majorList = await PageList<Major, MajorViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = majorList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting deleted majors: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách ngành học đã xóa, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetMajorByIdAsync(int majorId)
        {
            try
            {
                var major = await _context.Majors.FirstOrDefaultAsync(m => m.Id == majorId);
                if (major is null || major.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy ngành học hoặc ngành học đã bị xóa"
                    };
                }
                var majorViewModel = _mapper.Map<MajorViewModel>(major);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = majorViewModel
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting major by id: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin ngành học, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> DeleteMajorAsync(int majorId)
        {
            try
            {
                var major = await _context.Majors.FirstOrDefaultAsync(m => m.Id == majorId);
                if (major is null || major.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy ngành học hoặc ngành học đã bị xóa"
                    };
                }
                major.IsDeleted = true;
                major.DeletedAt = DateTime.UtcNow;
                major.DeletedBy = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Majors.Update(major);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Xóa ngành học thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while deleting major: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa ngành học, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> RestoreMajorAsync(int majorId)
        {
            try
            {
                var major = await _context.Majors.FirstOrDefaultAsync(m => m.Id == majorId);
                if (major is null || major.IsDeleted == false)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy ngành học"
                    };
                }
                major.IsDeleted = false;
                _context.Majors.Update(major);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Khôi phục ngành học thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while restoring major: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục ngành học, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> UpdateMajorAsync(int majorId, UpdateMajorModel model)
        {
            try
            {
                var major = await _context.Majors.FirstOrDefaultAsync(m => m.Id == majorId);
                var existsMajorCode = await _context.Majors.AnyAsync(m => m.MajorCode == model.MajorCode && m.Id != majorId);
                if (major is null || major.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy ngành học hoặc ngành học đã bị xóa"
                    };
                }
                if (existsMajorCode)
                {
                    return new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Mã ngành học đã tồn tại"
                    };
                }

                major.MajorCode = model.MajorCode ?? major.MajorCode;
                major.MajorName = model.MajorName ?? major.MajorName;
                major.MinTrainingYears = model.MinTrainingYears ?? major.MinTrainingYears;
                major.MaxTrainingYears = model.MaxTrainingYears ?? major.MaxTrainingYears;
                _context.Majors.Update(major);
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Cập nhật thông tin ngành học thành công",
                    Data = _mapper.Map<MajorViewModel>(major)
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while updating major: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật ngành học, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }

        }
        public async Task<ActionResponse> SearchMajorsAsync(string keyword, int? limit)
        {
            try
            {
                var searchLimit = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _context.Majors.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(m => m.MajorCode != null && m.MajorCode.Contains(keyword) || m.MajorName.Contains(keyword));
                }
                query = query.Where(m => m.IsDeleted == false);
                var response = await query.Take(searchLimit).ToListAsync();
                var majorList = _mapper.Map<List<MajorViewModel>>(response);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = majorList
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while searching majors: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tìm kiếm ngành học, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
    }
}