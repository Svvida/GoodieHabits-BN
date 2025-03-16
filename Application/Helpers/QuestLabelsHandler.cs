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

        public async Task<QuestMetadata> HandlePatchLabelsAsync(
            QuestMetadata quest,
            BaseUpdateQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            var existingLabels = quest.QuestLabels.ToList();

            _logger.LogInformation("Existing labels: {@existingLabels}", existingLabels);

            HashSet<int> newLabelsHashSet = [.. updateDto.Labels];
            _logger.LogInformation("NewLabelsHashSet: {@newLabelsHashSet}.", newLabelsHashSet);
            HashSet<int> existingLabelsHashSet = [.. quest.QuestLabels.Select(x => x.QuestLabelId)];
            _logger.LogInformation("ExistingLabelsHashSet: {@existingLabelsHashSet}.", existingLabelsHashSet);

            var labelsToAdd = updateDto.Labels
                .Where(labelId => !existingLabelsHashSet.Contains(labelId))
                .Select(labelId => new QuestMetadata_QuestLabel
                {
                    QuestMetadataId = quest.Id,
                    QuestLabelId = labelId
                }).ToList();

            foreach (var item in labelsToAdd)
            {
                var label = await _questLabelRepository.GetLabelByIdAsync(item.QuestLabelId, quest.AccountId, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"QuestLabel with ID: {item.QuestLabelId} not found");
            }

            var labelsToRemove = existingLabels
                .Where(existingLabel => !newLabelsHashSet.Contains(existingLabel.QuestLabelId))
                .ToList();

            _logger.LogInformation("Labels to add: {@labelsToAdd}", labelsToAdd);
            _logger.LogInformation("Labels to remove: {@labelsToRemove}", labelsToRemove);

            await _questMetadataRepository.AddQuestLabelsAsync(labelsToAdd, cancellationToken);
            await _questMetadataRepository.RemoveQuestLabelsAsync(labelsToRemove, cancellationToken);

            return quest;
        }
    }
}
