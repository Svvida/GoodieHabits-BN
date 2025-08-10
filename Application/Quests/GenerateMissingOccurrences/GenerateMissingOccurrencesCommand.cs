using MediatR;

namespace Application.Quests.GenerateMissingOccurrences
{
    public record GenerateMissingOccurrencesCommand() : IRequest<int>;
}
