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
        private readonly IQuestMetadataRepository _questMetadataRepository;

        public QuestLabelsHandler(
            IQuestLabelRepository questLabelRepository,
            ILogger<QuestLabelsHandler> logger,
            IQuestMetadataRepository questMetadataRepository)
        {
            _questLabelRepository = questLabelRepository;
            _logger = logger;
            _questMetadataRepository = questMetadataRepository;
        }

        public async Task<QuestMetadata> HandleUpdateLabelsAsync(
            QuestMetadata quest,
            BaseUpdateQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            foreach (var labelId in updateDto.Labels)
            {
                bool isOwner = await _questLabelRepository.IsLabelOwnedByUserAsync(labelId, quest.AccountId, cancellationToken).ConfigureAwait(false);
                if (!isOwner)
                    throw new ForbiddenException($"Label with ID: {labelId} does not belong to the user.");
            }

            var existingLabels = quest.QuestLabels.ToList();

            HashSet<int> newLabelsHashSet = [.. updateDto.Labels];
            HashSet<int> existingLabelsHashSet = [.. quest.QuestLabels.Select(x => x.QuestLabelId)];

            var labelsToAdd = updateDto.Labels
                .Where(labelId => !existingLabelsHashSet.Contains(labelId))
                .Select(labelId => new QuestMetadata_QuestLabel
                {
                    QuestMetadataId = quest.Id,
                    QuestLabelId = labelId
                }).ToList();

            var labelsToRemove = existingLabels
                .Where(existingLabel => !newLabelsHashSet.Contains(existingLabel.QuestLabelId))
                .ToList();

            await _questMetadataRepository.AddQuestLabelsAsync(labelsToAdd, cancellationToken);
            await _questMetadataRepository.RemoveQuestLabelsAsync(labelsToRemove, cancellationToken);

            return quest;
        }
    }
}
