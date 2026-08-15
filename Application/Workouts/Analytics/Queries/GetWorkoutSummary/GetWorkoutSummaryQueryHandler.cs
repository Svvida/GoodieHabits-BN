using Application.Workouts.Analytics.Dtos;
using Domain.Calculators;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Workouts.Analytics.Queries.GetWorkoutSummary
{
    /// <summary>
    /// Aggregates in memory over the sessions in the range, mirroring how the finance analytics handlers work.
    /// The arithmetic itself lives in <see cref="WorkoutVolumeCalculator"/>, so it is unit-tested without a
    /// database — the InMemory provider would happily pass a sum that real SQL gets wrong.
    /// </summary>
    public class GetWorkoutSummaryQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetWorkoutSummaryQuery, WorkoutSummaryDto>
    {
        public async Task<WorkoutSummaryDto> Handle(GetWorkoutSummaryQuery request, CancellationToken cancellationToken)
        {
            var sessions = await unitOfWork.WorkoutSessions
                .GetForPeriodAsync(request.UserProfileId, request.From, request.To, cancellationToken)
                .ConfigureAwait(false);

            // Only finished training counts: an abandoned session is not work you did, and an in-progress one
            // would make the numbers move under the user as they log.
            var completed = sessions
                .Where(s => s.Status == WorkoutSessionStatusEnum.Completed)
                .ToList();

            var totals = completed.Select(s => WorkoutVolumeCalculator.CalculateSessionTotals(s)).ToList();
            var durations = completed.Select(s => s.DurationSeconds).Where(d => d.HasValue).Select(d => d!.Value).ToList();

            var muscleGroups = await BuildMuscleGroupBreakdownAsync(completed, cancellationToken).ConfigureAwait(false);

            return new WorkoutSummaryDto
            {
                From = request.From,
                To = request.To,
                SessionCount = completed.Count,
                ExerciseCount = totals.Sum(t => t.ExerciseCount),
                SetCount = totals.Sum(t => t.SetCount),
                TotalReps = totals.Sum(t => t.TotalReps),
                TotalVolume = totals.Sum(t => t.TotalVolume),
                TotalDurationSeconds = durations.Sum(),
                AverageDurationSeconds = durations.Count > 0 ? (int)durations.Average() : null,
                ByMuscleGroup = muscleGroups,
            };
        }

        private async Task<List<MuscleGroupVolumeDto>> BuildMuscleGroupBreakdownAsync(
            IReadOnlyList<WorkoutSession> sessions, CancellationToken cancellationToken)
        {
            var entries = sessions.SelectMany(s => s.Exercises).ToList();

            if (entries.Count == 0)
                return [];

            // The muscle group lives on the library row, not on the session snapshot, so it is resolved at read
            // time. Entries whose exercise has been deleted fall back to Other rather than vanishing.
            var exerciseIds = entries
                .Where(e => e.ExerciseId.HasValue)
                .Select(e => e.ExerciseId!.Value)
                .Distinct()
                .ToList();

            var byId = new Dictionary<int, MuscleGroupEnum>();

            if (exerciseIds.Count > 0)
            {
                var exercises = await unitOfWork.Exercises
                    .GetVisibleByIdsAsync(exerciseIds, sessions[0].UserProfileId, cancellationToken)
                    .ConfigureAwait(false);

                byId = exercises.ToDictionary(e => e.Id, e => e.MuscleGroup);
            }

            return [.. entries
                .Select(entry => new
                {
                    MuscleGroup = entry.ExerciseId is int id && byId.TryGetValue(id, out var group)
                        ? group
                        : MuscleGroupEnum.Other,
                    Sets = entry.Sets.Where(s => !s.IsWarmup).ToList(),
                })
                .GroupBy(x => x.MuscleGroup)
                .Select(g => new MuscleGroupVolumeDto
                {
                    MuscleGroup = g.Key,
                    SetCount = g.Sum(x => x.Sets.Count),
                    TotalReps = g.Sum(x => x.Sets.Sum(s => s.Reps ?? 0)),
                    TotalVolume = g.Sum(x => WorkoutVolumeCalculator.CalculateVolume(x.Sets)),
                })
                .Where(g => g.SetCount > 0)
                .OrderByDescending(g => g.TotalVolume)
                .ThenBy(g => g.MuscleGroup)];
        }
    }
}
