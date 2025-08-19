using Mapster;

namespace Application.Auth.Commands.Register
{
    public class RegisterMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RegisterRequest, RegisterCommand>();
        }
    }
}
