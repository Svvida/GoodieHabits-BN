using Mapster;

namespace Application.Accounts.Commands.ForgotPassword
{
    public class ForgotPasswordMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ForgotPasswordRequest, ForgotPasswordCommand>();
        }
    }
}
