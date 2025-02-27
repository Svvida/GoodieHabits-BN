using Application.Dtos.Quests.QuestMetadata;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Interfaces;

namespace Application.Services.Quests
{
    public class QuestMetadataService : IQuestMetadataService
    {
        private readonly IQuestMetadataRepository _repository;
        private readonly IMapper _mapper;

        public QuestMetadataService(IQuestMetadataRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GetQuestMetadataDto>> GetAllQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetTodaysQuestsAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetQuestMetadataDto>>(quests);
        }
    }
}
