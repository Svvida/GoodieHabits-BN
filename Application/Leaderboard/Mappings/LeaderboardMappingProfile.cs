using Application.Leaderboard.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Leaderboard.Mappings
{
    public class LeaderboardMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, LeaderboardItemDto>()
                .Map(dest => dest.Xp, src => src.TotalXp);
        }
    }
}
