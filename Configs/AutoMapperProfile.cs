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
            CreateMap<Course, CourseViewModel>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FirstName + " " + src.CreatedBy.LastName));
            CreateMap<Major, MajorViewModel>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FirstName + " " + src.CreatedBy.LastName));
            CreateMap<TrainingProgram, ProgramViewModel>()
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major.MajorName))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FirstName + " " + src.CreatedBy.LastName));
            CreateMap<PrimaryClass, ClassViewModel>()
            .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => src.Program.ProgramName))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FirstName + " " + src.CreatedBy.LastName));
            CreateMap<Organization, OrganizationViewModel>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FirstName + " " + src.CreatedBy.LastName));
            CreateMap<Student, StudentViewModel>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FirstName + " " + src.CreatedBy.LastName : null))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
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
        }
    }
}