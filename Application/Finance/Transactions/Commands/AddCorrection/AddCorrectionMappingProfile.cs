using Mapster;

namespace Application.Finance.Transactions.Commands.AddCorrection
{
    public class AddCorrectionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // TransactionId and UserProfileId are attached in the controller (route + authenticated identity).
            config.NewConfig<AddCorrectionRequest, AddCorrectionCommand>();
        }
    }
}
