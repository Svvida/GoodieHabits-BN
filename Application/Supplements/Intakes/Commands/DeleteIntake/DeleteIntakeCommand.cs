using Application.Common.Interfaces;
using Application.Supplements.Intakes.Dtos;

namespace Application.Supplements.Intakes.Commands.DeleteIntake
{
    public record DeleteIntakeCommand(int IntakeId, int UserProfileId) : ICommand<SupplementChecklistDto>;
}
