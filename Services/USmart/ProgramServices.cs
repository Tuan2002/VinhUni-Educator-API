
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
    public class ProgramServices : IProgramServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<ProgramServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        public ProgramServices(ApplicationDBContext context, IConfiguration config, ILogger<ProgramServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
        }
        public async Task<ActionResponse> SyncProgramsAsync()
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
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncTrain);
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
                                        ""value"": 1 // Đại học chính quy
                                    }
                                ],
                                ""sorts"": [
                                    {
                                        ""field"": ""created"",
                                        ""dir"": -1
                                    }
                                ]
                                }";
                var responseData = await fetch.FetchAsync("gwsg/dbdaotao_chinhquy/tbl_ChuongTrinhDaoTao/getPaged", null, formBody, Method.Post);
                List<ProgramSyncModel> listProgram = JsonSerializer.Deserialize<List<ProgramSyncModel>>(responseData?.data?.ToString());
                if (responseData?.success == false || listProgram is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy danh sách chương trình đào tạo từ hệ thống USmart"
                    };
                }
                // Update or insert training program to database
                int countNewProgram = 0;
                int countFailed = 0;
                await _context.Database.BeginTransactionAsync();
                foreach (var item in listProgram)
                {
                    var course = await _context.Courses.FirstOrDefaultAsync(o => o.CourseId == item.idKhoaHoc);
                    var major = await _context.Majors.FirstOrDefaultAsync(o => o.MajorId == item.idNganh);
                    var program = await _context.TrainingPrograms.FirstOrDefaultAsync(o => o.ProgramId == item.id);
                    if (program is null && major != null && course != null)
                    {
                        program = new TrainingProgram
                        {
                            ProgramId = item.id,
                            ProgramCode = item.code,
                            ProgramName = item.ten,
                            MajorId = major.Id,
                            CourseId = course.Id,
                            CreditHours = item.soTinChi,
                            StartYear = item.namBatDau,
                            TrainingYears = item.soNamDaoTao,
                            MaxTrainingYears = item.soNamDaoTaoToiDa,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _context.TrainingPrograms.AddAsync(program);
                        countNewProgram++;
                    }
                    if (major is null || course is null)
                        countFailed++;
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncTrain,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm {countNewProgram} chương trình đào tạo mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                _context.SaveChanges();
                var message = countNewProgram > 0 ? $"Đã cập nhật thêm {countNewProgram} chương trình đào tạo mới và {countFailed} chưa được cập nhật" : "Không có chương trình đào tạo nào mới";
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = message,
                };
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                _logger.LogError($"Error occurred while syncing training: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ chương trình đào tạo, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }

    }
}