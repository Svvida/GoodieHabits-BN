using Application.Quests.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.RemoveQuestCompletion
{
    public class RemoveQuestCompletionCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMapper,
        IClock clock)
        : IRequestHandler<RemoveQuestCompletionCommand, QuestDetailsDto>
    {
        public async Task<QuestDetailsDto> Handle(RemoveQuestCompletionCommand command, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests
                .GetQuestForCompletionAsync(command.QuestId, command.UserProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {command.QuestId} not found");

            var completion = quest.Completions.FirstOrDefault(c => c.Id == command.CompletionId)
                ?? throw new NotFoundException($"Completion with ID: {command.CompletionId} not found on this quest.");

            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();
            var today = quest.UserProfile.LocalDateOn(nowUtc);

            quest.RemoveCompletion(completion, nowUtc, today);
            unitOfWork.QuestCompletions.Remove(completion);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return questMapper.MapToDto(quest, today);
        }
    }
}
