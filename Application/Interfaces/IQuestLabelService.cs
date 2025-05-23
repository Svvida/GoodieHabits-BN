﻿using Application.Dtos.Labels;

namespace Application.Interfaces
{
    public interface IQuestLabelService
    {
        Task<IEnumerable<GetQuestLabelDto>> GetUserLabelsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<int> CreateLabelAsync(CreateQuestLabelDto createDto, CancellationToken cancellationToken = default);
        Task PatchLabelAsync(int labelId, PatchQuestLabelDto patchDto, CancellationToken cancellationToken = default);
        Task DeleteLabelAsync(int labelId, CancellationToken cancellationToken = default);
    }
}
