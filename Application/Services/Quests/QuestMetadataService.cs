using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class QuestMetadataService : IQuestMetadataService
    {
        private readonly IQuestMetadataRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestMetadataService> _logger;

        public QuestMetadataService(
            IQuestMetadataRepository repository,
            IMapper mapper,
            ILogger<QuestMetadataService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<object>> GetAllQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetTodaysQuestsAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Quests before mapping: {@quests}", quests);

            var mappedQuests = quests.Select(q =>
            {
                return q.GetActualQuest() switch
                {
                    OneTimeQuest oneTime => QuestDto.From(_mapper.Map<GetOneTimeQuestDto>(oneTime)).Value,
                    DailyQuest daily => QuestDto.From(_mapper.Map<GetDailyQuestDto>(daily)).Value,
                    WeeklyQuest weekly => QuestDto.From(_mapper.Map<GetWeeklyQuestDto>(weekly)).Value,
                    MonthlyQuest monthly => QuestDto.From(_mapper.Map<GetMonthlyQuestDto>(monthly)).Value,
                    SeasonalQuest seasonal => QuestDto.From(_mapper.Map<GetSeasonalQuestDto>(seasonal)).Value,
                    _ => throw new ArgumentException("Unknown QuestType")
                };
            }).ToList();


            _logger.LogInformation("Quests after mapping: {@mappedQests}", mappedQuests);

            return mappedQuests;
        }
    }
}
