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
    public class CourseServices : ICourseServices
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<CourseServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtServices _jwtServices;
        private readonly IMapper _mapper;
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const int DEFAULT_SEARCH_RESULT = 10;
        public CourseServices(ApplicationDBContext context, IConfiguration config, ILogger<CourseServices> logger, IHttpContextAccessor contextAccessor, IJwtServices jwtServices, IMapper mapper)
        {
            _httpContextAccessor = contextAccessor;
            _context = context;
            _config = config;
            _logger = logger;
            _jwtServices = jwtServices;
            _mapper = mapper;
        }
        public async Task<ActionResponse> SyncCoursesAsync()
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
                var lastSync = await _context.SyncActions.OrderByDescending(s => s.SyncAt).FirstOrDefaultAsync(s => s.ActionName == SyncActionList.SyncCourse);
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
                                        ""field"": ""ten"",
                                        ""dir"": 1
                                    }
                                ]
                                }";
                var responseData = await fetch.FetchAsync("gwsg/dbdaotao_chinhquy/tbl_DM_KhoaHoc/getPaged", null, formBody, Method.Post);
                List<CourseSyncModel> listCourse = JsonSerializer.Deserialize<List<CourseSyncModel>>(responseData?.data?.ToString());
                if (responseData?.success == false || listCourse is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Có lỗi xảy ra khi lấy danh sách khoá đào tạo từ hệ thống USmart"
                    };
                }
                // Update or insert training course to database
                int countNewCourse = 0;
                foreach (var item in listCourse)
                {
                    var course = await _context.Courses.FirstOrDefaultAsync(o => o.CourseId == item.id);
                    if (course is null)
                    {
                        course = new Course
                        {
                            CourseId = item.id,
                            CourseCode = item.code,
                            CourseName = item.ten,
                            StartYear = item.namBatDau,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _context.Courses.AddAsync(course);
                        countNewCourse++;
                    }
                }
                // Log sync action
                var newSyncAction = new SyncAction
                {
                    ActionName = SyncActionList.SyncCourse,
                    SyncAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = true,
                    Message = $"Đã cập nhật thêm {countNewCourse} khoá đào tạo mới vào lúc: {DateTime.UtcNow}",
                };
                await _context.SyncActions.AddAsync(newSyncAction);
                _context.SaveChanges();
                var message = countNewCourse > 0 ? $"Đã cập nhật thêm {countNewCourse} khoá đào tạo mới" : "Không có khoá đào tạo nào mới";
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = message
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while syncing courses: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi đồng bộ khoá đào tạo, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetCoursesAsync(int? pageIndex, int? limit)
        {
            try
            {
                var query = _context.Courses.AsQueryable();
                query = query.Where(c => c.IsDeleted == false);
                var currentPageIndex = pageIndex ?? DEFAULT_PAGE_INDEX;
                var currentLimit = limit ?? DEFAULT_PAGE_SIZE;
                var listCourse = await PageList<Course, CourseViewModel>.CreateWithMapperAsync(query, currentPageIndex, currentLimit, _mapper);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = listCourse
                };

            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting courses: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách khoá đào tạo, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
                if (course is null || course.IsDeleted == true)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy khoá đào tạo hoặc khoá đào tạo đã bị xóa"
                    };
                }
                var courseModel = _mapper.Map<CourseViewModel>(course);
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Data = courseModel
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting course by id: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin khoá đào tạo, vui lòng thử lại sau hoặc liên hệ quản trị viên"
                };
            }
        }
        public async Task<ActionResponse> DeleteCourseAsync(int courseId)
        {
            try
            {
                var course = _context.Courses.FirstOrDefault(c => c.Id == courseId && c.IsDeleted == false);
                if (course is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy khoá đào tạo"
                    };
                }
                course.IsDeleted = true;
                course.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Xóa khoá đào tạo thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while deleting course: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa khoá đào tạo, vui lòng thử lại sau!"
                };
            }

        }
        public async Task<ActionResponse> RestoreCourseAsync(int courseId)
        {
            try
            {
                var course = _context.Courses.FirstOrDefault(c => c.Id == courseId && c.IsDeleted == true);
                if (course is null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Không tìm thấy khoá đào tạo"
                    };
                }
                course.IsDeleted = false;
                course.DeletedAt = null;
                await _context.SaveChangesAsync();
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Khôi phục khoá đào tạo thành công"
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while restoring course: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi khôi phục khoá đào tạo, vui lòng thử lại sau!"
                };
            }
        }
        public async Task<ActionResponse> SearchCourseAsync(string keyword, int? limit)
        {
            try
            {
                var searchLimit = limit ?? DEFAULT_SEARCH_RESULT;
                var query = _context.Courses.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(c => c.CourseCode.Contains(keyword) || c.CourseName.Contains(keyword));
                }
                query = query.Where(c => c.IsDeleted == false);
                query = query.OrderBy(c => c.CourseCode);
                var response = await query.Take(searchLimit).ToListAsync();
                var listCourse = _mapper.Map<List<CourseViewModel>>(response);
                var TotalCount = listCourse.Count;
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = $"Tìm thấy {TotalCount} khoá đào tạo",
                    Data = new
                    {
                        listCourse,
                        TotalCount
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while searching courses: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tìm kiếm khoá đào tạo"
                };
            }
        }
    }
}