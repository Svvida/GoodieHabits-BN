using Mapster;

namespace Application.Finance.Transactions.Commands.CreateTransaction
{
    public class CreateTransactionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<CreateTransactionRequest, CreateTransactionCommand>();
        }
    }
}
