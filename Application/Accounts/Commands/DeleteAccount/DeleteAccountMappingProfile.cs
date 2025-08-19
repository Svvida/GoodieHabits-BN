using Mapster;

namespace Application.Accounts.Commands.DeleteAccount
{
    public class DeleteAccountMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DeleteAccountRequest, DeleteAccountCommand>();
        }
    }
}
