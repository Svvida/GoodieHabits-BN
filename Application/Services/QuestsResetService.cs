using Application.Interfaces.Quests;
using Domain.Interfaces;

namespace Application.Services
{
    public class QuestsResetService : IQuestResetService
    {
        private readonly IResetQuestsRepository _resetQuestRepository;

        public QuestsResetService(IResetQuestsRepository resetQuestRepository)
        {
            _resetQuestRepository = resetQuestRepository;
        }

        public async Task ResetDailyQuestsAsync(CancellationToken cancellationToken = default)
        {
            await _resetQuestRepository.ResetDailyQuestsAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
