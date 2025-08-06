using Mapster;

namespace Application.Auth.RefreshAccessToken
{
    public class RefreshAccessTokenMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RefreshAccessTokenRequest, RefreshAccessTokenCommand>();
        }
    }
}
