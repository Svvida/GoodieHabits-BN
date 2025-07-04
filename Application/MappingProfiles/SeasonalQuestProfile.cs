﻿using Application.Dtos.Quests;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class SeasonalQuestProfile : Profile
    {
        public SeasonalQuestProfile()
        {
            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<Quest, GetSeasonalQuestDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.QuestType))
                .ForMember(dest => dest.Season, opt => opt.MapFrom(src => src.SeasonalQuest_Season!.Season.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty.ToString()))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Quest_QuestLabels));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateSeasonalQuestDto, Quest>()
                .ForMember(dest => dest.Quest_QuestLabels, opt => opt.MapFrom(src => src.Labels.Select(ql => new Quest_QuestLabel
                {
                    QuestLabelId = ql
                }).ToList()))
                .ForMember(dest => dest.SeasonalQuest_Season, opt => opt.MapFrom(src => new SeasonalQuest_Season
                {
                    Season = Enum.Parse<SeasonEnum>(src.Season, true)
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.Difficulty, opt => opt.ConvertUsing(new NullableEnumConverter<DifficultyEnum>(), src => src.Difficulty));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<QuestCompletionPatchDto, Quest>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateSeasonalQuestDto, Quest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.Difficulty, opt => opt.ConvertUsing(new NullableEnumConverter<DifficultyEnum>(), src => src.Difficulty))
                .ForPath(dest => dest.SeasonalQuest_Season!.Season, opt => opt.MapFrom(src => Enum.Parse<SeasonEnum>(src.Season, true)));
        }
    }
}
