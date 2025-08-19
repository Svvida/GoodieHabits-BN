using Application.Common.Interfaces;

namespace Application.Quests.Commands.GenerateMissingOccurrences
{
    public record GenerateMissingOccurrencesCommand() : ICommand<int>;
}
