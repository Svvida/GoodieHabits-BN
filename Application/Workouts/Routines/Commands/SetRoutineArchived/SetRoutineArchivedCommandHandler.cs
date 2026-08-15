using Application.Workouts.Routines.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Routines.Commands.SetRoutineArchived
{
    /// <summary>
    /// Hides a routine from the "start a session" picker without deleting it or its history. The gentler
    /// alternative to delete for a plan the user has simply moved on from.
    /// </summary>
    public class SetRoutineArchivedCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<SetRoutineArchivedCommand, WorkoutRoutineDto>
    {
        public async Task<WorkoutRoutineDto> Handle(SetRoutineArchivedCommand request, CancellationToken cancellationToken)
        {
            var routine = await unitOfWork.WorkoutRoutines
                .GetOwnedByIdAsync(request.RoutineId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Routine with ID {request.RoutineId} not found.");

            routine.SetArchived(request.IsArchived);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<WorkoutRoutineDto>(routine);
        }
    }
}
