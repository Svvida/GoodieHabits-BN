using Mapster;

namespace Application.Accounts.Commands.WipeoutData
{
    public class WipeoutDataMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WipeoutDataRequest, WipeoutDataCommand>();
        }
    }
}
