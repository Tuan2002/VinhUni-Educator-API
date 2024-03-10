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
        }
    }
}