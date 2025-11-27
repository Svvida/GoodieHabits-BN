using Domain.Common;
using Domain.Enums;
using Domain.Events.Badges;
using Domain.Exceptions;
using Domain.ValueObjects;
using NodaTime;

namespace Domain.Models
{
    public class UserProfile : EntityBase
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string TimeZone { get; private set; } = "Etc/UTC";
        public string Nickname { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public int TotalXp { get; set; } = 0;
        public int Coins { get; set; } = 0;
        // Stats for quests
        public int CompletedQuests { get; set; } = 0;
        public int CompletedDailyQuests { get; set; } = 0;
        public int CompletedWeeklyQuests { get; set; } = 0;
        public int CompletedMonthlyQuests { get; set; } = 0;
        public int TotalQuests { get; set; } = 0;
        public int ExistingQuests { get; set; } = 0;
        public int CurrentlyCompletedExistingQuests { get; set; } = 0;
        public int EverCompletedExistingQuests { get; set; } = 0;
        // Stats for goals
        public int CompletedGoals { get; set; } = 0;
        public int ExpiredGoals { get; set; } = 0;
        public int TotalGoals { get; set; } = 0;
        public int ActiveGoals { get; set; } = 0;

        // Stats for friends
        public int FriendsCount { get; set; } = 0;

        public Account Account { get; set; } = null!;
        public ICollection<Quest> Quests { get; set; } = [];
        public ICollection<QuestLabel> Labels { get; set; } = [];
        public ICollection<UserGoal> UserGoals { get; set; } = [];
        public ICollection<UserProfile_Badge> UserProfile_Badges { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<FriendInvitation> SentFriendInvitations { get; private set; } = [];
        public ICollection<FriendInvitation> ReceivedFriendInvitations { get; private set; } = [];
        public ICollection<UserBlock> SentBlocks { get; private set; } = [];
        public ICollection<UserBlock> ReceivedBlocks { get; private set; } = [];
        public ICollection<Friendship> FriendshipsAsUser1 { get; private set; } = [];
        public ICollection<Friendship> FriendshipsAsUser2 { get; private set; } = [];
        public ICollection<UserInventory> InventoryItems { get; private set; } = [];
        public ICollection<ActiveUserEffect> ActiveUserEffects { get; private set; } = [];

        public UserProfile() { }
        public UserProfile(Account account, string nickname, string timeZone = "Etc/Utc")
        {
            Account = account ?? throw new InvalidArgumentException("Account cannot be null.");
            TimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone)?.Id
                ?? throw new Exceptions.InvalidTimeZoneException(Id, timeZone);
            Nickname = nickname ?? throw new InvalidArgumentException("Nickname cannot be null.");
        }

        public void UpdateTimeZone(string? timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
                throw new InvalidArgumentException("TimeZone cannot be null or whitespace.");
            TimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone)?.Id
                ?? throw new Exceptions.InvalidTimeZoneException(Id, timeZone);
        }

        public void WipeoutData()
        {
            Bio = null;
            TotalXp = 0;

            // Stats for quests
            CompletedQuests = 0;
            CompletedDailyQuests = 0;
            CompletedWeeklyQuests = 0;
            CompletedMonthlyQuests = 0;
            TotalQuests = 0;
            ExistingQuests = 0;
            CurrentlyCompletedExistingQuests = 0;
            EverCompletedExistingQuests = 0;

            // Stats for goals
            CompletedGoals = 0;
            ExpiredGoals = 0;
            TotalGoals = 0;
            ActiveGoals = 0;

            UserProfile_Badges.Clear();
        }

        public void ApplyQuestCompletionRewards(int xpAwarded, bool isGoalCompleted, bool isFirstTimeCompleted, bool shouldAssignRewards, QuestTypeEnum questType)
        {
            if (shouldAssignRewards)
            {
                CompletedQuests++;
                TotalXp += xpAwarded;
            }
            if (isFirstTimeCompleted)
                EverCompletedExistingQuests++;
            if (isGoalCompleted)
                CompletedGoals++;
            CurrentlyCompletedExistingQuests++;

            switch (questType)
            {
                case QuestTypeEnum.Daily:
                    CompletedDailyQuests++;
                    break;
                case QuestTypeEnum.Weekly:
                    CompletedWeeklyQuests++;
                    break;
                case QuestTypeEnum.Monthly:
                    CompletedMonthlyQuests++;
                    break;
            }
        }

        public void RevertQuestCompletion(QuestTypeEnum questType)
        {
            CurrentlyCompletedExistingQuests = Math.Max(CurrentlyCompletedExistingQuests - 1, 0);
            switch (questType)
            {
                case QuestTypeEnum.Daily:
                    CompletedDailyQuests = Math.Max(CompletedDailyQuests - 1, 0);
                    break;
                case QuestTypeEnum.Weekly:
                    CompletedWeeklyQuests = Math.Max(CompletedWeeklyQuests - 1, 0);
                    break;
                case QuestTypeEnum.Monthly:
                    CompletedMonthlyQuests = Math.Max(CompletedMonthlyQuests - 1, 0);
                    break;
            }
        }

        public void UpdateAfterQuestDeletion(
            bool isQuestCompleted,
            bool isQuestEverCompleted,
            bool isQuestActiveGoal)
        {
            ExistingQuests = Math.Max(ExistingQuests - 1, 0);
            if (isQuestCompleted)
                CurrentlyCompletedExistingQuests = Math.Max(CurrentlyCompletedExistingQuests - 1, 0);
            if (isQuestEverCompleted)
                EverCompletedExistingQuests = Math.Max(EverCompletedExistingQuests - 1, 0);
            if (isQuestActiveGoal)
                ActiveGoals = Math.Max(ActiveGoals - 1, 0);
        }

        public void UpdateAfterQuestCreation()
        {
            ExistingQuests++;
            TotalQuests++;
        }

        public void UpdateAfterUserGoalCreation()
        {
            ActiveGoals++;
            TotalGoals++;
        }

        public void UpdateNickname(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new InvalidArgumentException("Nickname cannot be null or whitespace.");
            Nickname = nickname;
        }

        public void UpdateBio(string? bio)
        {
            if (bio != null && bio.Length > 30)
                throw new InvalidArgumentException("Bio cannot exceed 30 characters.");
            Bio = bio;
        }

        public void IncrementExpiredGoals(int count)
        {
            if (count <= 0)
                return;

            ExpiredGoals += count;
            ActiveGoals = Math.Max(ActiveGoals - count, 0);
        }

        public void DecrementCompletedQuestsAfterReset(int count)
        {
            if (count <= 0)
                return;

            CurrentlyCompletedExistingQuests = Math.Max(CurrentlyCompletedExistingQuests - count, 0);
        }

        public int ExpireGoals(DateTime nowUtc)
        {
            int expiredCount = 0;
            foreach (var goal in UserGoals)
            {
                if (goal.Expire(nowUtc))
                    expiredCount++;
            }
            IncrementExpiredGoals(expiredCount);
            return expiredCount;
        }

        public int ResetQuests(DateTime nowUtc)
        {
            int resetCount = 0;
            foreach (var quest in Quests)
            {
                if (quest.ResetCompletedStatus(nowUtc))
                    resetCount++;
            }
            DecrementCompletedQuestsAfterReset(resetCount);
            return resetCount;
        }

        public void AwardBadge(Badge badge, DateTime utcNow)
        {
            if (UserProfile_Badges.Any(upb => upb.BadgeId == badge.Id))
                return; // Badge already awarded
            UserProfile_Badges.Add(new UserProfile_Badge(this, badge, utcNow));

            AddDomainEvent(new BadgeAwardedEvent(Id, badge));
        }

        public void IncreaseFriendsCount() => FriendsCount++;

        public void DecreaseFriendsCount() => FriendsCount = Math.Max(FriendsCount - 1, 0);

        public void PurchaseItem(ShopItem shopItem, int userLevel, DateTime nowUtc)
        {
            if (userLevel < shopItem.LevelRequirement)
                throw new PurchaseItemException("Insufficient level to purchase this item.");

            if (Coins < shopItem.Price)
                throw new PurchaseItemException("Insufficient funds.");

            var existingItem = InventoryItems.FirstOrDefault(ii => ii.ShopItemId == shopItem.Id);

            if (shopItem.IsUnique)
            {
                if (existingItem is not null)
                    throw new PurchaseItemException("You already own this item.");

                Coins -= shopItem.Price;
                var newItem = UserInventory.Create(Id, shopItem.Id, 1, nowUtc);
                InventoryItems.Add(newItem);
            }
            else
            {
                Coins -= shopItem.Price;

                if (existingItem is not null)
                {
                    existingItem.IncreaseQuantity(1);
                }
                else
                {
                    var newItem = UserInventory.Create(Id, shopItem.Id, 1, nowUtc);
                    InventoryItems.Add(newItem);
                }
            }
        }

        public void GrandItem(ShopItem item, DateTime utcNow)
        {
            var existingItem = InventoryItems.FirstOrDefault(ii => ii.ShopItemId == item.Id);
            if (item.IsUnique)
            {
                if (existingItem is null)
                {
                    InventoryItems.Add(UserInventory.Create(Id, item.Id, 1, utcNow));
                }
            }
            else
            {
                if (existingItem is not null)
                    existingItem.IncreaseQuantity(1);
                else
                    InventoryItems.Add(UserInventory.Create(Id, item.Id, 1, utcNow));
            }
        }

        public void UseConsumableItem(UserInventory item, DateTime utcNow)
        {
            if (item.ShopItem.Category != ShopItemsCategoryEnum.Consumables)
                throw new InvalidArgumentException("Only consumable items can be used.");

            if (item.ShopItem.Payload is not ConsumablePayload payload)
            {
                throw new InvalidArgumentException($"Item {item.ShopItem.Name} does not have valid consumable configuration.");
            }

            item.ConsumeItem();

            var activeEffectValues = new ActiveEffectValues
            {
                FlatValue = payload.FlatValue,
                Multiplier = payload.Multiplier
            };

            var expiresAt = payload.DurationMinutes.HasValue
                ? utcNow.AddMinutes(payload.DurationMinutes.Value)
                : (DateTime?)null;

            var activeEffect = ActiveUserEffect.Create(
                Id,
                item.ShopItemId,
                payload.EffectType,
                expiresAt,
                payload.UsageCount,
                activeEffectValues);

            ActiveUserEffects.Add(activeEffect);
        }
    }
}