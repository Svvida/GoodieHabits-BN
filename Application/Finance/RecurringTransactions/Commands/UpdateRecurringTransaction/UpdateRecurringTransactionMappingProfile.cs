using Mapster;

namespace Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction
{
    public class UpdateRecurringTransactionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // RecurringTransactionId and UserProfileId are attached in the controller.
            config.NewConfig<UpdateRecurringTransactionRequest, UpdateRecurringTransactionCommand>();
        }
    }
}
