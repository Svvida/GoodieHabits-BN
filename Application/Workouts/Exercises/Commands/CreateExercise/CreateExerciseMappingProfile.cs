using Mapster;

namespace Application.Workouts.Exercises.Commands.CreateExercise
{
    public class CreateExerciseMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<CreateExerciseRequest, CreateExerciseCommand>();
        }
    }
}
