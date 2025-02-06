using Application.Dtos.QuestMetadata;
using Application.Helpers;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class QuestMetadataProfile : Profile
    {
        public QuestMetadataProfile()
        {
            CreateMap<QuestMetadata, QuestMetadataDto>()
                .ForMember(dest => dest.QuestType, opt => opt.MapFrom(src => src.QuestType.ToString()))
                .ForMember(dest => dest.Quest, opt => opt.MapFrom<QuestMetadataResolver>());
        }
    }
}
