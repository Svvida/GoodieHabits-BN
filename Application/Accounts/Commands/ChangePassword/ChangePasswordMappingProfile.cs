using Mapster;

namespace Application.Accounts.Commands.ChangePassword
{
    public class ChangePasswordMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ChangePasswordRequest, ChangePasswordCommand>();
        }
    }
}
