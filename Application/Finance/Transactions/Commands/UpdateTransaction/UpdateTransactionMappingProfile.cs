using Mapster;

namespace Application.Finance.Transactions.Commands.UpdateTransaction
{
    public class UpdateTransactionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // TransactionId (route) and UserProfileId (identity) are attached in the controller.
            config.NewConfig<UpdateTransactionRequest, UpdateTransactionCommand>();
        }
    }
}
