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
    public class PrimaryClassServices : IPrimaryClassServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PrimaryClassServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        public PrimaryClassServices(ApplicationDBContext context, IConfiguration config, ILogger<PrimaryClassServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
        }
        public async Task<ActionResponse> SyncPrimaryClassesAsync()
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
                //Check if the last sync action is within 30 minutes
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncClass);
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
                                        ""field"": ""idHe"",
                                        ""operator"": ""eq"",
                                        ""value"": ""CQ"" // Đại học chính quy
                                    }
                                ],
                                ""fields"": ""id,code,ten,idNganh,idKhoaHoc"",
                                }";
                var responseData = await fetch.FetchAsync("gwsg/dbnguoihoc/tbl_DanhSach_LopHanhChinh/getPaged", null, formBody, Method.Post);
                List<ClassSyncModel> listClass = JsonSerializer.Deserialize<List<ClassSyncModel>>(responseData?.data?.ToString());
                if (responseData?.success == false || listClass is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy danh sách lớp hành chính từ hệ thống USmart"
                    };
                }
                // Update or insert primary class to database
                int countNewClass = 0;
                int countFailed = 0;
                await _context.Database.BeginTransactionAsync();
                foreach (var item in listClass)
                {
                    var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == item.idKhoaHoc);
                    var program = await _context.TrainingPrograms.FirstOrDefaultAsync(p => p.ProgramCode == item.idNganh);
                    var primaryClass = await _context.PrimaryClasses.FirstOrDefaultAsync(cls => cls.ClassId == item.id);
                    if (primaryClass is null && program != null && course != null)
                    {
                        primaryClass = new PrimaryClass
                        {
                            ClassId = item.id,
                            ClassCode = item.code,
                            ClassName = item.ten,
                            ProgramId = program.Id,
                            CourseId = course.Id,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                        };

                        await _context.PrimaryClasses.AddAsync(primaryClass);
                        countNewClass++;
                    }
                    if (program is null || course is null)
                        countFailed++;
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncClass,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm {countNewClass} lớp hành chính mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                _context.SaveChanges();
                var message = countNewClass > 0 ? $"Đã cập nhật thêm {countNewClass} lớp mới và {countFailed} chưa được cập nhật" : "Không có lớp nào mới";
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = message,
                };
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                _logger.LogError($"Error occurred while syncing classes: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ lớp hành chính, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
    }
}