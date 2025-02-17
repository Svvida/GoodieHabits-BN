﻿using Application.Dtos.QuestMetadata;
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

        public async Task<IEnumerable<GetQuestMetadataDto>> GetAllQuestsAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetTodaysQuestsAsync(cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetQuestMetadataDto>>(quests);
        }
    }
}
