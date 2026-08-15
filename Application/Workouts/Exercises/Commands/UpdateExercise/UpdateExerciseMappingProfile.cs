using Mapster;

namespace Application.Workouts.Exercises.Commands.UpdateExercise
{
    public class UpdateExerciseMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // ExerciseId and UserProfileId are attached in the controller (route + identity).
            config.NewConfig<UpdateExerciseRequest, UpdateExerciseCommand>();
        }
    }
}
