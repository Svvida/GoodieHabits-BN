using Mapster;

namespace Application.Accounts.WipeoutData
{
    public class WipeoutDataMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WipeoutDataRequest, WipeoutDataCommand>();
        }
    }
}
