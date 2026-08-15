using Application.Workouts.Settings.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using MediatR;

namespace Application.Workouts.Settings.Commands.UpdateWeightUnit
{
    public class UpdateWeightUnitCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateWeightUnitCommand, WorkoutSettingsDto>
    {
        public async Task<WorkoutSettingsDto> Handle(UpdateWeightUnitCommand request, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles
                .GetByIdAsync(request.UserProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.UserProfileId} not found.");

            profile.UpdateWeightUnit(request.WeightUnit);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new WorkoutSettingsDto
            {
                WeightUnit = profile.WeightUnit,
                SupportedWeightUnits = [.. SupportedWeightUnits.All.OrderBy(u => u)],
            };
        }
    }
}
