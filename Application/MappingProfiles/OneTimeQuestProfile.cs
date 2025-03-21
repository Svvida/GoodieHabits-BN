﻿using Application.Dtos.Labels;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class OneTimeQuestProfile : Profile
    {
        public OneTimeQuestProfile()
        {
            CreateMap<QuestMetadata, GetOneTimeQuestDto>()
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src =>
                    src.QuestLabels.Select(ql => new GetQuestLabelDto
                    {
                        Id = ql.QuestLabel.Id,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor
                    }).ToList()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.OneTimeQuest!.Priority.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.OneTimeQuest!.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.OneTimeQuest!.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.OneTimeQuest!.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.OneTimeQuest!.EndDate))
                .ForMember(dest => dest.Emoji, opt => opt.MapFrom(src => src.OneTimeQuest!.Emoji))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.OneTimeQuest!.IsCompleted));

            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<OneTimeQuest, GetOneTimeQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Labels, opt => opt.Ignore());

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateOneTimeQuestDto, OneTimeQuest>()
                .ForMember(dest => dest.QuestMetadata, opt => opt.MapFrom(src => new QuestMetadata
                {
                    QuestType = QuestTypeEnum.OneTime,
                    AccountId = src.AccountId,
                    QuestLabels = src.Labels.Select(ql => new QuestMetadata_QuestLabel
                    {
                        QuestLabelId = ql
                    }).ToList()
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<PatchOneTimeQuestDto, OneTimeQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom((src, dest) => src.IsCompleted ?? dest.IsCompleted))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateOneTimeQuestDto, OneTimeQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
