﻿using Application.Dtos.Labels;
using Application.Dtos.Quests.DailyQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class DailyQuestProfile : Profile
    {
        public DailyQuestProfile()
        {
            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<Quest, GetDailyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.QuestType))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src =>
                    src.Quest_QuestLabels.Select(ql => new GetQuestLabelDto
                    {
                        Id = ql.QuestLabel.Id,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor
                    }).ToList()));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateDailyQuestDto, Quest>()
                .ForMember(dest => dest.Quest_QuestLabels, opt => opt.MapFrom(src => src.Labels.Select(ql => new Quest_QuestLabel
                {
                    QuestLabelId = ql
                }).ToList()))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<DailyQuestCompletionPatchDto, Quest>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateDailyQuestDto, Quest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
