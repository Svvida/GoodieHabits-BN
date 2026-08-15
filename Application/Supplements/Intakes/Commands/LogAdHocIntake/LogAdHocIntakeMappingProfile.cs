using Mapster;

namespace Application.Supplements.Intakes.Commands.LogAdHocIntake
{
    public class LogAdHocIntakeMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<LogAdHocIntakeRequest, LogAdHocIntakeCommand>();
        }
    }
}
