using Application.Workouts.Routines.Dtos;
using Domain.Enums;
using Domain.Models;
using Mapster;

namespace Application.Workouts.Routines.Mappings
{
    public class WorkoutRoutineMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // The Exercise navigation is always included by IWorkoutRoutineRepository, but the mapping stays
            // null-safe: a routine DTO built from a partially loaded graph should degrade, not throw.
            config.NewConfig<WorkoutRoutineExercise, WorkoutRoutineExerciseDto>()
                .Map(dest => dest.ExerciseName, src => src.Exercise != null ? src.Exercise.Name : string.Empty)
                .Map(dest => dest.MetricType, src => src.Exercise != null ? src.Exercise.MetricType : ExerciseMetricEnum.Reps)
                .Map(dest => dest.MuscleGroup, src => src.Exercise != null ? src.Exercise.MuscleGroup : MuscleGroupEnum.Other);

            config.NewConfig<WorkoutRoutine, WorkoutRoutineDto>();
        }
    }
}
