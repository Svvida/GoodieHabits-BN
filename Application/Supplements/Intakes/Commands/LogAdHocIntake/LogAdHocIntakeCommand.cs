using Application.Common.Interfaces;
using Application.Supplements.Intakes.Dtos;

namespace Application.Supplements.Intakes.Commands.LogAdHocIntake
{
    public record LogAdHocIntakeCommand(
        int SupplementId,
        DateOnly Date,
        decimal? Amount,
        int? WorkoutSessionId,
        int UserProfileId) : ICommand<SupplementChecklistDto>;
}
