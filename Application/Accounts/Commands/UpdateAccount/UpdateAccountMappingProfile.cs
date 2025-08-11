using Mapster;

namespace Application.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateAccountRequest, UpdateAccountCommand>();
        }
    }
}
