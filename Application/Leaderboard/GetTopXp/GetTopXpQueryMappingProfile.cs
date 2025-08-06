using Domain.Models;
using Mapster;

namespace Application.Leaderboard.GetTopXp
{
    public class GetTopXpQueryMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<IEnumerable<UserProfile>, GetTopXpResponse>()
                .Map(dest => dest.TopXp, src => src);
        }
    }
}
