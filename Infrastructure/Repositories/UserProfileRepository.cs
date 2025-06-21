using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserProfileRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> DoesNicknameExistAsync(string nickname, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles.AnyAsync(u => u.Nickname == nickname, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            var existingProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Id == userProfile.Id, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"User profile with ID: {userProfile.Id} not found.");

            _mapper.Map(userProfile, existingProfile);

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetUserProfileWithGoalsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Include(u => u.Account)
                    .ThenInclude(a => a.UserGoals)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
