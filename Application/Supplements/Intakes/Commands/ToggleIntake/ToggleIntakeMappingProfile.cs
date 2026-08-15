using Mapster;

namespace Application.Supplements.Intakes.Commands.ToggleIntake
{
    public class ToggleIntakeMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<ToggleIntakeRequest, ToggleIntakeCommand>();
        }
    }
}
