using Mapster;

namespace Application.Finance.Settings.Commands.UpdateCurrency
{
    public class UpdateCurrencyMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<UpdateCurrencyRequest, UpdateCurrencyCommand>();
        }
    }
}
