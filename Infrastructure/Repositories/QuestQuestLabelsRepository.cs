using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class QuestQuestLabelsRepository : IQuestQuestLabelsRepository
    {
        private readonly AppDbContext _context;
        public QuestQuestLabelsRepository(AppDbContext context)
        {
            _context = context;
        }
        public void AddRange(IEnumerable<Quest_QuestLabel> labels)
        {
            _context.Quest_QuestLabels.AddRange(labels);
        }
        public void RemoveRange(IEnumerable<Quest_QuestLabel> labels)
        {
            _context.Quest_QuestLabels.RemoveRange(labels);
        }
    }
}