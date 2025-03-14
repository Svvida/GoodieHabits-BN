using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class QuestService : IQuestService
    {
        private readonly IQuestMetadataRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestService> _logger;

        public QuestService(
            IQuestMetadataRepository repository,
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
            var quests = await _repository.GetActiveQuestsAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Quests before mapping: {@quests}", quests);

            var questList = quests
                .Select(q =>
                    (BaseGetQuestDto)(q.GetActualQuest() switch
                    {
                        OneTimeQuest oneTime => _mapper.Map<GetOneTimeQuestDto>(q),
                        DailyQuest daily => _mapper.Map<GetDailyQuestDto>(q),
                        WeeklyQuest weekly => _mapper.Map<GetWeeklyQuestDto>(q),
                        MonthlyQuest monthly => _mapper.Map<GetMonthlyQuestDto>(q),
                        SeasonalQuest seasonal => _mapper.Map<GetSeasonalQuestDto>(q),
                        _ => null
                    })!
                )
                .Where(q => q is not null)
                .ToList();

            _logger.LogInformation("Quests after mapping: {@mappedQuests}", questList);

            return questList;
        }
    }
}

