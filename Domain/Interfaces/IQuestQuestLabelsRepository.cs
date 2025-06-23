using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestQuestLabelsRepository
    {
        void AddRange(IEnumerable<Quest_QuestLabel> labels);
        void RemoveRange(IEnumerable<Quest_QuestLabel> labels);
    }
}
