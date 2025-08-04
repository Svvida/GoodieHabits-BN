using Application.Leaderboard.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.Leaderboard.Mappings
{
    public class LeaderboardMappingProfile : Profile
    {
        public LeaderboardMappingProfile()
        {
            CreateMap<UserProfile, LeaderboardItemDto>()
                .ForCtorParam(nameof(LeaderboardItemDto.Xp), opt => opt.MapFrom(src => src.TotalXp));
        }
    }
}
