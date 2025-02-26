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

        public void ResetDailyQuestsAsync(CancellationToken cancellationToken = default)
        {
            _resetQuestRepository.ResetDailyQuestsAsync(cancellationToken);
        }
    }
}
