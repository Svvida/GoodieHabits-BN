﻿using Application.Dtos.Labels;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class WeeklyQuestProfile : Profile
    {
        public WeeklyQuestProfile()
        {
            CreateMap<QuestMetadata, GetWeeklyQuestDto>()
                .ForMember(dest => dest.QuestLabels, opt => opt.MapFrom(src =>
                    src.QuestLabels.Select(ql => new GetQuestLabelDto
                    {
                        Id = ql.QuestLabel.Id,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor
                    }).ToList()))
                .ForMember(dest => dest.Weekdays, opt => opt.MapFrom(src => src.WeeklyQuest!.Weekdays.ConvertAll(w => w.ToString())))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.WeeklyQuest!.Priority.ToString()));

            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<WeeklyQuest, GetWeeklyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Weekdays, opt => opt.MapFrom(src => src.Weekdays.ConvertAll(w => w.ToString())))
                .ForMember(dest => dest.QuestLabels, opt => opt.Ignore());

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateWeeklyQuestDto, WeeklyQuest>()
                .ForMember(dest => dest.QuestMetadata, opt => opt.MapFrom(src => new QuestMetadata
                {
                    QuestType = QuestTypeEnum.Weekly,
                    AccountId = src.AccountId
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.Weekdays, opt => opt.MapFrom(src => src.Weekdays.ConvertAll(w => Enum.Parse<WeekdayEnum>(w))));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<PatchWeeklyQuestDto, WeeklyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom((src, dest) => src.IsCompleted ?? dest.IsCompleted))
                .ForMember(dest => dest.Weekdays, opt => opt.MapFrom(src => src.Weekdays!.ConvertAll(w => Enum.Parse<WeekdayEnum>(w))))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateWeeklyQuestDto, WeeklyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.Weekdays, opt => opt.MapFrom(src => src.Weekdays.ConvertAll(w => Enum.Parse<WeekdayEnum>(w))));
        }
    }
}
