using Application.Workouts.Routines.Dtos;
using Mapster;

namespace Application.Workouts.Routines.Commands.UpdateRoutine
{
    public class UpdateRoutineMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // RoutineId and UserProfileId are attached in the controller (route + identity).
            config.NewConfig<UpdateRoutineRequest, UpdateRoutineCommand>()
                .Map(dest => dest.Exercises, src => src.Exercises ?? new List<RoutineExerciseInput>());
        }
    }
}
