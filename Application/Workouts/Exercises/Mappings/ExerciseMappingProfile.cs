using Application.Workouts.Exercises.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Workouts.Exercises.Mappings
{
    public class ExerciseMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Exercise, ExerciseDto>();
        }
    }
}
