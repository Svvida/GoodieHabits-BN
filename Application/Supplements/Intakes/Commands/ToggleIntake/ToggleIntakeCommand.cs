using Application.Common.Interfaces;
using Application.Supplements.Intakes.Dtos;

namespace Application.Supplements.Intakes.Commands.ToggleIntake
{
    public record ToggleIntakeCommand(
        int SupplementId,
        int SlotId,
        DateOnly Date,
        bool Taken,
        decimal? Amount,
        int? WorkoutSessionId,
        int UserProfileId) : ICommand<SupplementChecklistDto>;
}
