using Application.Workouts.Settings.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using MediatR;

namespace Application.Workouts.Settings.Queries.GetWorkoutSettings
{
    public class GetWorkoutSettingsQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetWorkoutSettingsQuery, WorkoutSettingsDto>
    {
        public async Task<WorkoutSettingsDto> Handle(GetWorkoutSettingsQuery request, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles
                .GetByIdAsync(request.UserProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.UserProfileId} not found.");

            // The allow-list ships with the settings so the client never hard-codes it.
            return new WorkoutSettingsDto
            {
                WeightUnit = profile.WeightUnit,
                SupportedWeightUnits = [.. SupportedWeightUnits.All.OrderBy(u => u)],
            };
        }
    }
}
