using Domain.Exceptions;

namespace Domain.Models
{
    public class UserInventory
    {
        public int Id { get; private set; }
        public int UserProfileId { get; private set; }
        public int ShopItemId { get; private set; }
        public int Quantity { get; private set; } = 1;
        public DateTime AcquiredAt { get; private set; } = DateTime.UtcNow;
        public bool IsActive { get; private set; } = false;

        public UserProfile UserProfile { get; private set; } = null!;
        public ShopItem ShopItem { get; private set; } = null!;

        protected UserInventory() { }
        private UserInventory(int userProfileId, int shopItemId, int quantity, DateTime acquiredAt)
        {
            UserProfileId = userProfileId;
            ShopItemId = shopItemId;
            Quantity = quantity;
            AcquiredAt = acquiredAt;
        }

        public static UserInventory Create(int userProfileId, int shopItemId, int quantity, DateTime acquiredAt)
        {
            if (quantity <= 0)
                throw new InvalidArgumentException("Quantity of a item cannot be 0 or less.");
            return new UserInventory(userProfileId, shopItemId, quantity, acquiredAt);
        }

        public void IncreaseQuantity(int amount)
        {
            if (amount <= 0)
                throw new InvalidArgumentException("Amount must be greater than 0.");
            Quantity += amount;
        }
    }
}
