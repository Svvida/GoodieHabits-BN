using AutoMapper;

namespace Application.Auth.RefreshAccessToken
{
    public class RefreshAccessTokenMappingProfile : Profile
    {
        public RefreshAccessTokenMappingProfile()
        {
            CreateMap<RefreshAccessTokenRequest, RefreshAccessTokenCommand>()
                .ForMember(src => src.TimeZoneId, opt => opt.Ignore());
        }
    }
}
