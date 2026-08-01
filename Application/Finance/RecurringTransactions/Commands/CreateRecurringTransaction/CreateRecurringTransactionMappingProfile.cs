using Mapster;

namespace Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction
{
    public class CreateRecurringTransactionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached in the controller from the authenticated identity.
            config.NewConfig<CreateRecurringTransactionRequest, CreateRecurringTransactionCommand>();
        }
    }
}
