using AutoMapper;
using VinhUni_Educator_API.Entities;
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
        }
    }
}