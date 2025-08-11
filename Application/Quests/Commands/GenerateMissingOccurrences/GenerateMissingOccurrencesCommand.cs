using MediatR;

namespace Application.Quests.Commands.GenerateMissingOccurrences
{
    public record GenerateMissingOccurrencesCommand() : IRequest<int>;
}
