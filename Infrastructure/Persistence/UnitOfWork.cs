﻿using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Persistence
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        private IAccountRepository? _accountRepository;
        private IUserProfileRepository? _userProfileRepository;
        private IUserGoalRepository? _userGoalRepository;
        private IQuestRepository? _questRepository;
        private IQuestLabelRepository? _questLabelRepository;

        public IAccountRepository Accounts => _accountRepository ??= new AccountRepository(_context);
        public IUserProfileRepository UserProfiles => _userProfileRepository ??= new UserProfileRepository(_context);
        public IUserGoalRepository UserGoals => _userGoalRepository ??= new UserGoalRepository(_context);
        public IQuestRepository Quests => _questRepository ??= new QuestRepository(_context);
        public IQuestLabelRepository QuestLabels => _questLabelRepository ??= new QuestLabelRepository(_context);

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