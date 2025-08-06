using Mapster;

namespace Application.Accounts.DeleteAccount
{
    public class DeleteAccountMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DeleteAccountRequest, DeleteAccountCommand>();
        }
    }
}
