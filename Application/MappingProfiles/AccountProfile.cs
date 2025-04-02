using Application.Dtos.Accounts;
using Application.Dtos.Labels;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            // Entity -> DTO
            CreateMap<Account, GetAccountDto>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => new GetAccountDataDto
                {
                    QuestsLabels = src.Labels.Select(label => new GetQuestLabelDto
                    {
                        Id = label.Id,
                        Value = label.Value,
                        BackgroundColor = label.BackgroundColor,
                        TextColor = label.TextColor
                    }).ToList()
                }));

            // Create DTO -> Entity
            CreateMap<CreateAccountDto, Account>()
                .ForMember(dest => dest.HashPassword, opt => opt.MapFrom(src => src.Password));

            // Patch DTO -> Entity
            CreateMap<PatchAccountDto, Account>()
                .ForAllMembers(opt => opt.Condition((src, member, srcMember) => srcMember != null));
        }
    }
}
