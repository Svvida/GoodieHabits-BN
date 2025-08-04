using AutoMapper;
using Domain.Models;

namespace Application.Leaderboard.GetTopXp
{
    public class GetTopXpQueryMappingProfile : Profile
    {
        public GetTopXpQueryMappingProfile()
        {
            CreateMap<IEnumerable<UserProfile>, GetTopXpResponse>()
                .ForCtorParam(nameof(GetTopXpResponse.TopXp), opt => opt.MapFrom(src => src));
        }
    }
}
