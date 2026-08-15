using Application.Workouts.Analytics.Dtos;
using Domain.Calculators;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Workouts.Analytics.Queries.GetExerciseHistory
{
    /// <summary>
    /// Progress on one exercise, one point per completed session, oldest first.
    /// <para>
    /// This is where the estimated one-rep max is computed: the rows are already in memory, so Epley keeps its
    /// exact semantics (a single rep returns the weight itself, very high rep counts return nothing) instead of
    /// being approximated by a SQL expression.
    /// </para>
    /// </summary>
    public class GetExerciseHistoryQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetExerciseHistoryQuery, ExerciseHistoryDto>
    {
        public async Task<ExerciseHistoryDto> Handle(GetExerciseHistoryQuery request, CancellationToken cancellationToken)
        {
            var exercise = await unitOfWork.Exercises
                .GetVisibleByIdAsync(request.ExerciseId, request.UserProfileId, true, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found.");

            var sessions = await unitOfWork.WorkoutSessions
                .GetForExerciseAsync(request.UserProfileId, request.ExerciseId, request.From, request.To, cancellationToken)
                .ConfigureAwait(false);

            var points = new List<ExerciseHistoryPointDto>();

            foreach (var session in sessions.Where(s => s.Status == WorkoutSessionStatusEnum.Completed))
            {
                var sets = session.Exercises
                    .Where(e => e.ExerciseId == request.ExerciseId)
                    .SelectMany(e => e.Sets)
                    .Where(s => !s.IsWarmup)
                    .ToList();

                if (sets.Count == 0)
                    continue;

                points.Add(new ExerciseHistoryPointDto
                {
                    SessionId = session.Id,
                    PerformedOn = session.PerformedOn,
                    SetCount = sets.Count,
                    TotalReps = sets.Sum(s => s.Reps ?? 0),
                    TotalVolume = WorkoutVolumeCalculator.CalculateVolume(sets),
                    MaxWeight = sets.Max(s => s.Weight),
                    BestEstimatedOneRepMax = OneRepMaxCalculator.BestEstimate(sets),
                });
            }

            return new ExerciseHistoryDto
            {
                ExerciseId = exercise.Id,
                ExerciseName = exercise.Name,
                From = request.From,
                To = request.To,
                Points = points,
            };
        }
    }
}
