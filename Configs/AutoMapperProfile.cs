using AutoMapper;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Configs
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RefreshToken, RefreshTokenModel>();
            CreateMap<ApplicationUser, PublicUserModel>();
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<Course, CourseViewModel>();
            CreateMap<Major, MajorViewModel>();
            CreateMap<TrainingProgram, ProgramViewModel>()
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major.MajorName))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));
            CreateMap<PrimaryClass, ClassViewModel>()
            .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => src.Program.ProgramName))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));
            CreateMap<Organization, OrganizationViewModel>();

            // Mapper for Student
            CreateMap<Student, StudentViewModel>()
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.PrimaryClass != null ? src.PrimaryClass.ClassName : null))
            .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => src.TrainingProgram != null ? src.TrainingProgram.ProgramName : null))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));
            CreateMap<StudentSyncModel, ImportStudentModel>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.code))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.ho))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.ten))
            .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ngaySinh)))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => ConvertGender.ConvertToInt(src.gioiTinh)))
            .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.idLopHanhChinh))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.idKhoaHoc))
            .ForMember(dest => dest.ProgramCode, opt => opt.MapFrom(src => src.idNganh))
            .ForMember(dest => dest.SSOId, opt => opt.MapFrom(src => int.Parse(src.userId)));
            CreateMap<ImportStudentModel, ImportStudentViewModel>();

            // Mapper for Teacher
            CreateMap<Teacher, TeacherViewModel>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.OrganizationName : null));
            CreateMap<TeacherSyncModel, ImportTeacherModel>()
            .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.TeacherCode, opt => opt.MapFrom(src => src.hS_ID))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.hS_Ho))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.hS_Ten))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.hS_GioiTinh))
            .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ngaySinh)))
            .ForMember(dest => dest.OrganizationCode, opt => opt.MapFrom(src => src.dV_ID_GiangDay))
            .ForMember(dest => dest.SSOId, opt => opt.MapFrom(src => int.Parse(src.userId)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.hS_Email));
            CreateMap<ImportTeacherModel, ImportTeacherViewModel>();

            // Mapper for SchoolYear
            CreateMap<SchoolYear, SchoolYearViewModel>();
            CreateMap<Semester, SemesterViewModel>()
            .ForMember(dest => dest.SchoolYearName, opt => opt.MapFrom(src => src.SchoolYear.SchoolYearName));

            // Mapper for Module
            CreateMap<Module, ModuleViewModel>()
            .ForMember(dest => dest.ApplyYearName, opt => opt.MapFrom(src => src.ApplyYear != null ? src.ApplyYear.SchoolYearName : null));

            // Mapper for ModuleClass
            CreateMap<ModuleClass, ClassModuleViewModel>()
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module.ModuleName))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.FirstName + " " + src.Teacher.LastName))
            .ForMember(dest => dest.CurrentStudents, opt => opt.MapFrom(src => src.ModuleClassStudents.Count));

            // Mapper for category
            CreateMap<Category, CategoryViewModel>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName))
            .ForMember(dest => dest.IsShared, opt => opt.MapFrom(src => src.Owner.UserId != src.CreatedById))
            .ForMember(dest => dest.IsSharing, opt => opt.MapFrom(src => src.ShareCategories.Count > 0))
            .ForMember(dest => dest.SharedAt, opt => opt.MapFrom(src => src.ShareCategories.Where(sc => sc.CategoryId == src.Id).Select(sc => sc.SharedAt).FirstOrDefault()))
            .ForMember(dest => dest.ShareUntil, opt => opt.MapFrom(src => src.ShareCategories.Where(sc => sc.CategoryId == src.Id).Select(sc => sc.SharedUntil).FirstOrDefault()));

            // Mapper for question kit
            CreateMap<QuestionKit, QuestionKitViewModel>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.ModifiedByName, opt => opt.MapFrom(src => src.ModifiedBy != null ? src.ModifiedBy.FirstName + " " + src.ModifiedBy.LastName : null))
            .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.Questions.Count));
        }
    }
}