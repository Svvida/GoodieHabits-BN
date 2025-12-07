using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Models
{
    public class ActiveUserEffect
    {
        public int Id { get; private set; }
        public int UserProfileId { get; private set; }
        public int SourceItemId { get; private set; }
        public ConsumablesEffectTypeEnum EffectType { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public int? UsageCount { get; private set; }
        public ActiveEffectValues Values { get; private set; } = null!;

        public UserProfile UserProfile { get; private set; } = null!;
        public ShopItem SourceItem { get; private set; } = null!;

        protected ActiveUserEffect() { }

        private ActiveUserEffect(int userProfileId, int sourceItemId, ConsumablesEffectTypeEnum effectType, DateTime? expiresAt, int? usageCount, ActiveEffectValues values)
        {
            UserProfileId = userProfileId;
            SourceItemId = sourceItemId;
            EffectType = effectType;
            ExpiresAt = expiresAt;
            UsageCount = usageCount;
            Values = values;
        }

        public static ActiveUserEffect Create(int userProfileId, int sourceItemId, ConsumablesEffectTypeEnum effectType, DateTime? expiresAt, int? usageCount, ActiveEffectValues values)
        {
            if (usageCount.HasValue && usageCount.Value <= 0)
                throw new InvalidArgumentException("Usage count must be positive.");

            if (values is null)
                throw new InvalidArgumentException("Values for active effect cannot be null.");

            return new ActiveUserEffect(userProfileId, sourceItemId, effectType, expiresAt, usageCount, values);
        }

        public bool DecreaseUsageCount()
        {
            if (UsageCount.HasValue)
            {
                if (UsageCount.Value <= 0)
                    return true;
                UsageCount -= 1;
                return UsageCount.Value == 0;
            }
            return false; // Infinite usage, time based
        }
    }
}
