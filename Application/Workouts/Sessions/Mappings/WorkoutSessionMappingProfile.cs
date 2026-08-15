using Application.Workouts.Sessions.Dtos;
using Domain.Calculators;
using Domain.Models;
using Mapster;

namespace Application.Workouts.Sessions.Mappings
{
    public class WorkoutSessionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Derived values are attached after the field-by-field copy rather than written into the mapping
            // expression, so the calculators stay the single source of truth for both.
            config.NewConfig<WorkoutSet, WorkoutSetDto>()
                .AfterMapping((src, dest) =>
                    dest.EstimatedOneRepMax = OneRepMaxCalculator.Estimate(src.Reps, src.Weight));

            config.NewConfig<WorkoutSessionExercise, WorkoutSessionExerciseDto>();

            config.NewConfig<WorkoutSession, WorkoutSessionSummaryDto>()
                .AfterMapping((src, dest) =>
                    dest.Totals = WorkoutSessionTotalsDto.From(WorkoutVolumeCalculator.CalculateSessionTotals(src)));

            config.NewConfig<WorkoutSession, WorkoutSessionDto>()
                .AfterMapping((src, dest) =>
                    dest.Totals = WorkoutSessionTotalsDto.From(WorkoutVolumeCalculator.CalculateSessionTotals(src)));
        }
    }
}
