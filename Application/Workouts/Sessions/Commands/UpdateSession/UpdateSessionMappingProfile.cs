using Mapster;

namespace Application.Workouts.Sessions.Commands.UpdateSession
{
    public class UpdateSessionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // SessionId and UserProfileId are attached in the controller (route + identity).
            config.NewConfig<UpdateSessionRequest, UpdateSessionCommand>();
        }
    }
}
