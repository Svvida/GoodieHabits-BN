using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    public class ShopItem
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string ImageUrl { get; private set; } = null!;
        public ShopItemsCategoryEnum Category { get; private set; }
        public ShopItemTypeEnum ItemType { get; private set; }
        public int Price { get; private set; } // 0 indicates free item
        public CurrencyTypeEnum CurrencyType { get; private set; } = 0;
        public int LevelRequirement { get; private set; } = 1;
        public bool IsPurchasable { get; private set; } = true; // Can be used to hide certain items from the shop
        public bool IsUnique { get; private set; } // true for items a user can only own one of (cosmetics)
        public string? EffectDataJson { get; private set; }

        public ICollection<UserInventory> UserInventories { get; private set; } = [];

        protected ShopItem() { }
        private ShopItem(int id, string name, string description, string imageUrl, ShopItemsCategoryEnum category,
            ShopItemTypeEnum itemType, int price, CurrencyTypeEnum currencyType, int levelRequirement,
            bool isPurchasable, bool isUnique, string? effectDataJson)
        {
            Id = id;
            Name = name;
            Description = description;
            ImageUrl = imageUrl;
            Category = category;
            ItemType = itemType;
            Price = price;
            CurrencyType = currencyType;
            LevelRequirement = levelRequirement;
            IsPurchasable = isPurchasable;
            IsUnique = isUnique;
            EffectDataJson = effectDataJson;
        }

        public static ShopItem Create(int id, string name, string description, string imageUrl, ShopItemsCategoryEnum category,
            ShopItemTypeEnum itemType, int price, CurrencyTypeEnum currencyType, int levelRequirement,
            bool isPurchasable, bool isUnique, string? effectDataJson)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidArgumentException("Name cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidArgumentException("Description cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new InvalidArgumentException("ImageUrl cannot be null or whitespace.");
            if (price < 0)
                throw new InvalidArgumentException("Price cannot be negative.");
            if (levelRequirement < 1)
                throw new InvalidArgumentException("LevelRequirement must be at least 1.");

            return new ShopItem(id, name, description, imageUrl, category, itemType, price, currencyType,
                levelRequirement, isPurchasable, isUnique, effectDataJson);
        }
    }
}
