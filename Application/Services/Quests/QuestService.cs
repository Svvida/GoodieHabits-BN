using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Helpers;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Interfaces.Quests;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class QuestService : IQuestService
    {
        private readonly IQuestRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestService> _logger;

        public QuestService(
            IQuestRepository repository,
            IMapper mapper,
            ILogger<QuestService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(
            int accountId, CancellationToken cancellationToken = default)
        {
            SeasonEnum currentSeason = SeasonHelper.GetCurrentSeason();

            var quests = await _repository.GetActiveQuestsAsync(accountId, currentSeason, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Quests before mapping: {@quests}", quests);

            var questList = quests
                .Where(q => q != null)
                .Select(q =>
                    (BaseGetQuestDto)(q.QuestType switch
                    {
                        QuestTypeEnum.OneTime => _mapper.Map<GetOneTimeQuestDto>(q),
                        QuestTypeEnum.Daily => _mapper.Map<GetDailyQuestDto>(q),
                        QuestTypeEnum.Weekly => _mapper.Map<GetWeeklyQuestDto>(q),
                        QuestTypeEnum.Monthly => _mapper.Map<GetMonthlyQuestDto>(q),
                        QuestTypeEnum.Seasonal => _mapper.Map<GetSeasonalQuestDto>(q),
                        _ => throw new ArgumentException("Invalid quest type")
                    }))
                .ToList();

            _logger.LogInformation("Quests after mapping: {@mappedQuests}", questList);

            return questList;
        }

        public async Task DeleteQuestAsync(int questId, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteQuestByIdAsync(questId, cancellationToken).ConfigureAwait(false);
        }
    }
}

