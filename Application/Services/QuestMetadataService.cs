using Application.Dtos.QuestMetadata;
using Application.Interfaces;
using AutoMapper;
using Domain.Interfaces;

namespace Application.Services
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

        public async Task<IEnumerable<QuestMetadataDto>> GetAllQuestsAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllQuestsAsync(cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<QuestMetadataDto>>(quests);
        }
    }
}
