using Application.Statistics.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.Statistics.Mappings
{
    public class XpProgressMappingProfile : Profile
    {
        public XpProgressMappingProfile()
        {
            CreateMap<UserProfile, XpProgressDto>()
                .AfterMap<SetUserLevelAction>();
        }
    }
}
