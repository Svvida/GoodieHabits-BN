using Domain.Interfaces;
using Domain.Interfaces.Quests;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Quests;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IAccountRepository? _accountRepository;
        private IUserProfileRepository? _userProfileRepository;
        private IUserGoalRepository? _userGoalRepository;
        private IQuestRepository? _questRepository;
        private IQuestLabelRepository? _questLabelRepository;
        private IQuestOccurrenceRepository? _questOccurrenceRepository;
        private IQuestStatisticsRepository? _questStatisticsRepository;
        private IQuestQuestLabelsRepository? _questQuestLabelsRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IAccountRepository Accounts => _accountRepository ??= new AccountRepository(_context);
        public IUserProfileRepository UserProfiles => _userProfileRepository ??= new UserProfileRepository(_context);
        public IUserGoalRepository UserGoals => _userGoalRepository ??= new UserGoalRepository(_context);
        public IQuestRepository Quests => _questRepository ??= new QuestRepository(_context);
        public IQuestLabelRepository QuestLabels => _questLabelRepository ??= new QuestLabelRepository(_context);
        public IQuestOccurrenceRepository QuestOccurrences => _questOccurrenceRepository ??= new QuestOccurrenceRepository(_context);
        public IQuestStatisticsRepository QuestStatistics => _questStatisticsRepository ??= new QuestStatisticsRepository(_context);
        public IQuestQuestLabelsRepository Quest_QuestLabels => _questQuestLabelsRepository ??= new QuestQuestLabelsRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}