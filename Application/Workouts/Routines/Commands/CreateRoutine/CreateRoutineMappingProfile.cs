using Application.Workouts.Routines.Dtos;
using Mapster;

namespace Application.Workouts.Routines.Commands.CreateRoutine
{
    public class CreateRoutineMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller. A null Exercises
            // list maps to an empty one so the handler never has to null-check the collection.
            config.NewConfig<CreateRoutineRequest, CreateRoutineCommand>()
                .Map(dest => dest.Exercises, src => src.Exercises ?? new List<RoutineExerciseInput>());
        }
    }
}
