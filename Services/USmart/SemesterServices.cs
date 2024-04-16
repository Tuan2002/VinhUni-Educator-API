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
        private readonly ICacheServices _cacheServices;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_PAGE = 1;
        public SemesterServices(IJwtServices jwtServices, ApplicationDBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration config, ILogger<SemesterServices> logger, IMapper mapper, ICacheServices cacheServices)
        {
            _jwtServices = jwtServices;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _mapper = mapper;
            _logger = logger;
            _cacheServices = cacheServices;
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
        public async Task<ActionResponse> GetSchoolYearsAsync(int? pageIndex, int? limit, bool skipCache)
        {
            try
            {
                int currentPage = pageIndex ?? DEFAULT_PAGE;
                int limitRows = limit ?? DEFAULT_LIMIT;
                var schoolYears = null as PageList<SchoolYear, SchoolYearViewModel>;
                var query = _context.SchoolYears.AsQueryable();
                query = query.OrderByDescending(s => s.YearCode);
                query = query.Where(s => s.IsDeleted == false);
                var cacheKey = $"SchoolYears_{currentPage}_{limitRows}";
                if (skipCache == false)
                {
                    var cachedData = await _cacheServices.GetDataAsync<PageList<SchoolYear, SchoolYearViewModel>>(cacheKey);
                    if (cachedData == null)
                    {
                        schoolYears = await PageList<SchoolYear, SchoolYearViewModel>.CreateWithMapperAsync(query, currentPage, limitRows, _mapper);
                        _ = await _cacheServices.SetDataAsync(cacheKey, schoolYears, DateTime.UtcNow.AddDays(30));
                    }
                    schoolYears = cachedData ?? schoolYears;
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        IsSuccess = true,
                        Message = "Lấy dữ liệu năm học thành công",
                        Data = schoolYears
                    };
                }
                schoolYears = await PageList<SchoolYear, SchoolYearViewModel>.CreateWithMapperAsync(query, currentPage, limitRows, _mapper);
                _ = await _cacheServices.SetDataAsync(cacheKey, schoolYears, DateTime.UtcNow.AddDays(30));
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
        public async Task<ActionResponse> GetDeletedSchoolYearsAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPage = pageIndex ?? DEFAULT_PAGE;
                int limitRows = limit ?? DEFAULT_LIMIT;
                var query = _context.SchoolYears.AsQueryable();
                query = query.OrderByDescending(s => s.CreatedAt);
                query = query.Where(s => s.IsDeleted == true);
                var schoolYears = await PageList<SchoolYear, SchoolYearViewModel>.CreateWithMapperAsync(query, currentPage, limitRows, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy dữ liệu năm học đã xóa thành công",
                    Data = schoolYears
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.GetDeletedSchoolYearsAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy dữ liệu năm học đã xóa, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> GetDeletedSemestersAsync(int? pageIndex, int? limit)
        {
            try
            {
                int currentPage = pageIndex ?? DEFAULT_PAGE;
                int limitRows = limit ?? DEFAULT_LIMIT;
                var query = _context.Semesters.AsQueryable();
                query = query.OrderByDescending(s => s.CreatedAt);
                query = query.Where(s => s.IsDeleted == true);
                var semesters = await PageList<Semester, SemesterViewModel>.CreateWithMapperAsync(query, currentPage, limitRows, _mapper);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy dữ liệu học kỳ đã xóa thành công",
                    Data = semesters
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.GetDeletedSemestersAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy dữ liệu học kỳ đã xóa, vui lòng thử lại sau"
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
        public async Task<ActionResponse> GetSemestersBySchoolYearAsync(int schoolYearId, bool skipCache)
        {
            try
            {
                var schoolYear = await _context.SchoolYears.AnyAsync(s => s.Id == schoolYearId || s.IsDeleted == false);
                if (!schoolYear)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy năm học"
                    };
                }
                var query = _context.Semesters.AsQueryable();
                query = query.OrderBy(s => s.SemesterType);
                query = query.Where(s => s.SchoolYearId == schoolYearId);
                query = query.Where(s => s.IsDeleted == false);
                var semesters = new List<SemesterViewModel>();
                var cacheKey = $"Semesters_Year_{schoolYearId}";
                if (skipCache == false)
                {
                    var cachedData = await _cacheServices.GetDataAsync<List<SemesterViewModel>>(cacheKey);
                    if (cachedData == null)
                    {
                        var newRawSemesters = await query.ToListAsync();
                        semesters = _mapper.Map<List<SemesterViewModel>>(newRawSemesters);
                        if (semesters.Count > 0)
                            _ = await _cacheServices.SetDataAsync(cacheKey, semesters, DateTime.UtcNow.AddDays(30));
                    }
                    semesters = cachedData ?? semesters;
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        IsSuccess = true,
                        Message = "Lấy dữ liệu học kỳ thành công",
                        Data = semesters
                    };
                }
                var rawSemesters = await query.ToListAsync();
                semesters = _mapper.Map<List<SemesterViewModel>>(rawSemesters);
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
        public async Task<ActionResponse> GetSemesterByIdAsync(int semesterId)
        {
            try
            {
                var rawSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
                if (rawSemester is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học kỳ"
                    };
                }
                var semester = _mapper.Map<SemesterViewModel>(rawSemester);
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Lấy dữ liệu học kỳ thành công",
                    Data = semester
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.GetSemesterByIdAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy dữ liệu học kỳ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> DeleteSchoolYearAsync(int schoolYearId)
        {
            try
            {
                var schoolYear = await _context.SchoolYears.FirstOrDefaultAsync(s => s.Id == schoolYearId);
                if (schoolYear is null || schoolYear.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy năm học hoặc năm học đã bị xóa"
                    };
                }
                schoolYear.IsDeleted = true;
                schoolYear.DeletedBy = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                schoolYear.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa năm học thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.DeleteSchoolYearAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa năm học, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> DeleteSemesterAsync(int semesterId)
        {
            try
            {
                var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
                if (semester is null || semester.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học kỳ hoặc học kỳ đã bị xóa"
                    };
                }
                semester.IsDeleted = true;
                semester.DeletedBy = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                semester.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Xóa học kỳ thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.DeleteSemesterAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa học kỳ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> RestoreSchoolYearAsync(int schoolYearId)
        {
            try
            {
                var schoolYear = await _context.SchoolYears.FirstOrDefaultAsync(s => s.Id == schoolYearId);
                if (schoolYear is null || !schoolYear.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy năm học"
                    };
                }
                schoolYear.IsDeleted = false;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Khôi phục năm học thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.RestoreSchoolYearAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục năm học, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> RestoreSemesterAsync(int semesterId)
        {
            try
            {
                var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
                if (semester is null || !semester.IsDeleted)
                {
                    return new ActionResponse
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        IsSuccess = false,
                        Message = "Không tìm thấy học kỳ"
                    };
                }
                semester.IsDeleted = false;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = StatusCodes.Status200OK,
                    IsSuccess = true,
                    Message = "Khôi phục học kỳ thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in SemesterServices.RestoreSemesterAsync: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục học kỳ, vui lòng thử lại sau"
                };
            }
        }
    }
}