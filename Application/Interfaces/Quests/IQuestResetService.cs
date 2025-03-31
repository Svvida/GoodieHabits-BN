using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestResetService
    {
        DateTime? GetNextResetTimeUtc(Quest quest);
    }
}
