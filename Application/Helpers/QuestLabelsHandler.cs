using Application.Dtos.Quests;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Logging;

namespace Application.Helpers
{
    public class QuestLabelsHandler : IQuestLabelsHandler
    {
        private readonly IQuestLabelRepository _questLabelRepository;
        private readonly ILogger<QuestLabelsHandler> _logger;
        private readonly IQuestRepository _questRepository;

        public QuestLabelsHandler(
            IQuestLabelRepository questLabelRepository,
            ILogger<QuestLabelsHandler> logger,
            IQuestRepository questRepository)
        {
            _questLabelRepository = questLabelRepository;
            _logger = logger;
            _questRepository = questRepository;
        }

        public async Task<Quest> HandleUpdateLabelsAsync(
            Quest quest,
            BaseUpdateQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {

            foreach (var labelId in updateDto.Labels)
            {
                bool isOwner = await _questLabelRepository.IsLabelOwnedByUserAsync(labelId, quest.AccountId, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug($"AccountId: {quest.AccountId} and LabelId: {labelId}, isOwner?: {isOwner}");
                if (!isOwner)
                    throw new ForbiddenException($"Label with ID: {labelId} does not belong to the user.");
            }

            var existingLabels = quest.Quest_QuestLabels.ToList();

            HashSet<int> existingLabelsHashSet = [.. quest.Quest_QuestLabels.Select(x => x.QuestLabelId)];
            HashSet<int> newLabelsHashSet = [.. updateDto.Labels];

            _logger.LogDebug("Existing labels: {@existingLabels}", existingLabels);
            _logger.LogDebug("New labels: {@newLabels}", newLabelsHashSet);

            var labelsToAdd = updateDto.Labels
                .Where(labelId => !existingLabelsHashSet.Contains(labelId))
                .Select(labelId => new Quest_QuestLabel
                {
                    QuestId = quest.Id,
                    QuestLabelId = labelId
                }).ToList();

            var labelsToRemove = existingLabels
                .Where(existingLabel => !newLabelsHashSet.Contains(existingLabel.QuestLabelId))
                .ToList();

            _logger.LogDebug("Labels to add: {@labelsToAdd}", labelsToAdd);
            _logger.LogDebug("Labels to remove: {@labelsToRemove}", labelsToRemove);

            if (labelsToRemove.Count != 0)
            {
                _logger.LogDebug("Removing labels: {@labelsToRemove} from quest with id: {@quest.id}", labelsToRemove, quest.Id);
                _questRepository.RemoveQuestLabels(labelsToRemove);
            }
            if (labelsToAdd.Count != 0)
            {
                _logger.LogDebug("Adding labels: {@labelsToAdd} to quest with id: {@quest.id}", labelsToAdd, quest.Id);
                _questRepository.AddQuestLabels(labelsToAdd);
            }

            return quest;
        }
    }
}
