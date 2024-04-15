using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class SemesterServices : ISemesterServices
    {
        private readonly IJwtServices _jwtServices;
        private readonly ApplicationDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly ILogger<SemesterServices> _logger;
        private readonly IMapper _mapper;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_PAGE = 1;
        public SemesterServices(IJwtServices jwtServices, ApplicationDBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration config, ILogger<SemesterServices> logger, IMapper mapper)
        {
            _jwtServices = jwtServices;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ActionResponse> SyncSchoolYearsAsync()
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
                var response = await fetch.FetchAsync("/gwsg/dbdaotao_chinhquy/tbl_HeThong_NamHoc/GetAsync");
                List<SchoolYearSyncModel> schoolYearList = JsonSerializer.Deserialize<List<SchoolYearSyncModel>>(response?.data?.ToString());
                if (response?.success == false || schoolYearList == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Không thể lấy dữ liệu từ hệ thống USmart, vui lòng thử lại sau"
                    };
                }
                int newSchoolYearCount = 0;
                foreach (var item in schoolYearList)
                {
                    var schoolYear = await _context.SchoolYears.FirstOrDefaultAsync(s => s.SchoolYearId == item.id);
                    if (schoolYear is null)
                    {
                        schoolYear = new SchoolYear
                        {
                            SchoolYearId = item.id,
                            YearCode = item.nam,
                            SchoolYearName = item.ten,
                            StartDate = DateOnly.FromDateTime(item.tuNgay),
                            EndDate = DateOnly.FromDateTime(item.denNgay),
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.SchoolYears.AddAsync(schoolYear);
                        newSchoolYearCount++;
                    }
                    else
                    {
                        schoolYear.YearCode = item.nam;
                        schoolYear.SchoolYearName = item.ten;
                        schoolYear.StartDate = DateOnly.FromDateTime(item.tuNgay);
                        schoolYear.EndDate = DateOnly.FromDateTime(item.denNgay);
                        _context.SchoolYears.Update(schoolYear);
                    }
                }
                await _context.SaveChangesAsync();
                var message = newSchoolYearCount > 0 ? $"Đã thêm {newSchoolYearCount} năm học mới" : "Không có năm học mới nào được thêm";
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.SyncSchoolYearsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ dữ liệu năm học, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> SyncSemestersAsync()
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
                var response = await fetch.FetchAsync("/gwsg/dbdaotao_chinhquy/tbl_HeThong_HocKy/GetAsync");
                List<SemesterSyncModel> semesterList = JsonSerializer.Deserialize<List<SemesterSyncModel>>(response?.data?.ToString());
                if (response?.success == false || semesterList == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        IsSuccess = false,
                        Message = "Không thể lấy dữ liệu từ hệ thống USmart, vui lòng thử lại sau"
                    };
                }
                int newSemesterCount = 0;
                int countFailed = 0;
                foreach (var item in semesterList)
                {
                    var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.SemesterId == item.id);
                    var schoolYear = await _context.SchoolYears.FirstOrDefaultAsync(s => s.YearCode == item.namHoc);
                    if (schoolYear is null)
                    {
                        countFailed++;
                        continue;
                    }
                    if (semester is null)
                    {
                        semester = new Semester
                        {
                            SemesterId = item.id,
                            SemesterType = item.type,
                            SemesterName = item.ten,
                            SemesterShortName = item.tenRutGon,
                            StartDate = DateOnly.FromDateTime(item.tuNgay),
                            EndDate = DateOnly.FromDateTime(item.denNgay),
                            SchoolYearId = schoolYear.Id,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Semesters.AddAsync(semester);
                        newSemesterCount++;
                    }
                    else
                    {
                        semester.SemesterType = item.type;
                        semester.SemesterName = item.ten;
                        semester.SemesterShortName = item.tenRutGon;
                        semester.StartDate = DateOnly.FromDateTime(item.tuNgay);
                        semester.EndDate = DateOnly.FromDateTime(item.denNgay);
                        semester.SchoolYearId = schoolYear.Id;
                        _context.Semesters.Update(semester);
                    }
                }
                await _context.SaveChangesAsync();
                var message = newSemesterCount > 0 ? $"Đã thêm {newSemesterCount} học kỳ mới và {countFailed} chưa được cập nhật" : "Không có học kỳ mới nào được thêm";
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.SyncSemestersAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ dữ liệu học kỳ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetSchoolYearsAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPage = pageIndex ?? DEFAULT_PAGE;
                int limitRows = limit ?? DEFAULT_LIMIT;
                var query = _context.SchoolYears.AsQueryable();
                query = query.OrderByDescending(s => s.YearCode);
                query = query.Where(s => s.IsDeleted == false);
                var schoolYears = await PageList<SchoolYear, SchoolYearViewModel>.CreateWithMapperAsync(query, currentPage, limitRows, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy dữ liệu năm học thành công",
                    Data = schoolYears
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.GetSchoolYearsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy dữ liệu năm học, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetSemestersAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPage = pageIndex ?? DEFAULT_PAGE;
                int limitRows = limit ?? DEFAULT_LIMIT;
                var query = _context.Semesters.AsQueryable();
                query = query.OrderByDescending(s => s.CreatedAt);
                query = query.Where(s => s.IsDeleted == false);
                var semesters = await PageList<Semester, SemesterViewModel>.CreateWithMapperAsync(query, currentPage, limitRows, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy dữ liệu học kỳ thành công",
                    Data = semesters
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.GetSemestersAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy dữ liệu học kỳ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetSemestersBySchoolYearAsync(int schoolYearId)
        {
            try
            {
                var query = _context.Semesters.AsQueryable();
                query = query.OrderBy(s => s.SemesterType);
                query = query.Where(s => s.SchoolYearId == schoolYearId);
                var rawSemesters = await query.ToListAsync();
                var semesters = _mapper.Map<List<SemesterViewModel>>(rawSemesters);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy dữ liệu học kỳ thành công",
                    Data = semesters
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.GetSemestersBySchoolYearAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy dữ liệu học kỳ, vui lòng thử lại sau"
                };
            }
        }
    }
}