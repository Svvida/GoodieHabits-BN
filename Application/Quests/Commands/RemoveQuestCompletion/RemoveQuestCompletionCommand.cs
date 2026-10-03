using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Commands.RemoveQuestCompletion
{
    /// <summary>
    /// Undoes one recorded completion. Any reward already paid for the period stays paid and is never paid
    /// again — mistakes happen, and policing remove-and-re-add would cost more than it protects.
    /// </summary>
    public record RemoveQuestCompletionCommand(
        int QuestId,
        int CompletionId,
        int UserProfileId) : ICommand<QuestDetailsDto>, ICurrentUserQuestCommand;
}
