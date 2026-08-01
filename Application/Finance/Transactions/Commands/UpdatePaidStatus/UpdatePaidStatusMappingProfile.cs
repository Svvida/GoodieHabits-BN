using Mapster;

namespace Application.Finance.Transactions.Commands.UpdatePaidStatus
{
    public class UpdatePaidStatusMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // TransactionId and UserProfileId are attached in the controller (route + authenticated identity).
            config.NewConfig<UpdatePaidStatusRequest, UpdatePaidStatusCommand>();
        }
    }
}
