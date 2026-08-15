using Application.Workouts.Routines.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;

namespace Application.Workouts.Routines.Common
{
    internal static class RoutineResponseBuilder
    {
        /// <summary>
        /// Re-reads the routine after a write so the response carries each item's exercise name and metric.
        /// <para>
        /// Those live on the <c>Exercise</c> navigation, which the just-written graph does not have loaded —
        /// and attaching the no-tracking rows the handler validated against would make EF try to insert them.
        /// One extra round-trip on a rare write buys the guarantee that the write's response is byte-identical
        /// to the next <c>GET</c>.
        /// </para>
        /// </summary>
        public static async Task<WorkoutRoutineDto> ReadBackAsync(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            int routineId,
            int userProfileId,
            CancellationToken cancellationToken)
        {
            var saved = await unitOfWork.WorkoutRoutines
                .GetOwnedByIdAsync(routineId, userProfileId, true, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Routine with ID {routineId} not found.");

            return mapper.Map<WorkoutRoutineDto>(saved);
        }
    }
}
