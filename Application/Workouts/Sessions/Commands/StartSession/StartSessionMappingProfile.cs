using Mapster;

namespace Application.Workouts.Sessions.Commands.StartSession
{
    public class StartSessionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<StartSessionRequest, StartSessionCommand>();
        }
    }
}
