using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class ShopItemConfiguration : IEntityTypeConfiguration<ShopItem>
    {
        public void Configure(EntityTypeBuilder<ShopItem> builder)
        {
            builder.ToTable("ShopItems");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Category);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Category)
                .IsRequired();

            builder.Property(x => x.ItemType)
                .IsRequired();

            builder.Property(x => x.Price)
                .IsRequired();

            builder.Property(x => x.CurrencyType)
                .HasDefaultValue(CurrencyTypeEnum.Gold)
                .IsRequired();

            builder.Property(x => x.LevelRequirement)
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(x => x.IsPurchasable)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.IsUnique)
                .IsRequired();

            builder.Property(x => x.EffectDataJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.HasData(
                // -------------------------
                // FREE AVATAR FRAMES (Unlocked by Level)
                // -------------------------
                ShopItem.Create(
                    id: 100,
                    name: "Wooden Rookie Frame",
                    description: "Unlocked at Level 1. Made from 100% imaginary wood.",
                    imageUrl: "frames/wooden_rookie",
                    category: ShopItemsCategoryEnum.AvatarFrames,
                    itemType: ShopItemTypeEnum.Cosmetic,
                    price: 0,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 1,
                    isPurchasable: false,
                    isUnique: true,
                    effectDataJson: "{\"frameId\": \"wooden_rookie\"}"
                ),
                ShopItem.Create(
                    id: 101,
                    name: "Silver Striver Frame",
                    description: "Unlocked at Level 4. Clean, shiny, probably not real silver.",
                    imageUrl: "frames/silver_striver",
                    category: ShopItemsCategoryEnum.AvatarFrames,
                    itemType: ShopItemTypeEnum.Cosmetic,
                    price: 0,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 4,
                    isPurchasable: false,
                    isUnique: true,
                    effectDataJson: "{\"frameId\": \"silver_striver\"}"
                ),
                ShopItem.Create(
                    id: 102,
                    name: "Gold Grinder Frame",
                    description: "Unlocked at Level 8. You earned this—literally.",
                    imageUrl: "frames/gold_grinder",
                    category: ShopItemsCategoryEnum.AvatarFrames,
                    itemType: ShopItemTypeEnum.Cosmetic,
                    price: 0,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 8,
                    isPurchasable: false,
                    isUnique: true,
                    effectDataJson: "{\"frameId\": \"gold_grinder\"}"
                ),
                ShopItem.Create(
                    id: 103,
                    name: "Diamond Achiever Frame",
                    description: "Unlocked at Level 15. Shiny enough to motivate anyone.",
                    imageUrl: "frames/diamond_achiever",
                    category: ShopItemsCategoryEnum.AvatarFrames,
                    itemType: ShopItemTypeEnum.Cosmetic,
                    price: 0,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 15,
                    isPurchasable: false,
                    isUnique: true,
                    effectDataJson: "{\"frameId\": \"diamond_achiever\"}"
                ),
                // -------------------------
                // PAID AVATARS
                // -------------------------
                ShopItem.Create(
                    id: 200,
                    name: "Cat Programmer",
                    description: "A cat furiously typing on a keyboard. Appears to know more than you.",
                    imageUrl: "avatars/cat_programmer",
                    category: ShopItemsCategoryEnum.Avatars,
                    itemType: ShopItemTypeEnum.Avatar,
                    price: 300,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 1,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"avatarId\": \"cat_programmer\"}"
                ),
                ShopItem.Create(
                    id: 201,
                    name: "Giga-Chad Avatar",
                    description: "The peak of digital evolution. Jawline DLC included.",
                    imageUrl: "avatars/giga_chad",
                    category: ShopItemsCategoryEnum.Avatars,
                    itemType: ShopItemTypeEnum.Avatar,
                    price: 400,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 5,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"avatarId\": \"giga_chad\"}"
                ),
                ShopItem.Create(
                    id: 202,
                    name: "Wizard of Deadlines",
                    description: "A stressed wizard constantly casting 'EXTEND DEADLINE' spell.",
                    imageUrl: "avatars/wizard_deadlines",
                    category: ShopItemsCategoryEnum.Avatars,
                    itemType: ShopItemTypeEnum.Avatar,
                    price: 350,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 3,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"avatarId\": \"wizard_deadlines\"}"
                ),
                // -------------------------
                // PETS
                // -------------------------
                ShopItem.Create(
                    id: 300,
                    name: "Tiny Dragon",
                    description: "A baby dragon that follows you around. Harmless. Probably.",
                    imageUrl: "pets/tiny_dragon",
                    category: ShopItemsCategoryEnum.Pets,
                    itemType: ShopItemTypeEnum.Pet,
                    price: 1200,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 10,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"petId\": \"tiny_dragon\", \"animation\": \"hover\"}"
                ),
                ShopItem.Create(
                    id: 301,
                    name: "Floating Duck",
                    description: "A yellow duck that defies gravity. Scientists hate it.",
                    imageUrl: "pets/floating_duck",
                    category: ShopItemsCategoryEnum.Pets,
                    itemType: ShopItemTypeEnum.Pet,
                    price: 800,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 6,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"petId\": \"floating_duck\", \"animation\": \"float\"}"
                ),
                // -------------------------
                // TITLES
                // -------------------------
                ShopItem.Create(
                    id: 400,
                    name: "Certified Procrastinator",
                    description: "A title that proudly states what everyone already knew.",
                    imageUrl: "titles/procrastinator",
                    category: ShopItemsCategoryEnum.Titles,
                    itemType: ShopItemTypeEnum.Title,
                    price: 250,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 1,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"title\": \"Certified Procrastinator\"}"
                ),
                ShopItem.Create(
                    id: 401,
                    name: "Task Slayer",
                    description: "For those who finish tasks with extreme prejudice.",
                    imageUrl: "titles/slayer",
                    category: ShopItemsCategoryEnum.Titles,
                    itemType: ShopItemTypeEnum.Title,
                    price: 600,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 8,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"title\": \"Task Slayer\"}"
                ),
                ShopItem.Create(
                    id: 402,
                    name: "Lord of Deadlines",
                    description: "One title to postpone them all.",
                    imageUrl: "titles/lord_deadlines",
                    category: ShopItemsCategoryEnum.Titles,
                    itemType: ShopItemTypeEnum.Title,
                    price: 900,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 12,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"title\": \"Lord of Deadlines\"}"
                ),
                // -------------------------
                // NAME EFFECTS
                // -------------------------
                ShopItem.Create(
                    id: 500,
                    name: "Blue Neon Name Glow",
                    description: "Makes your username glow with neon blue energy.",
                    imageUrl: "nameeffects/neon_blue",
                    category: ShopItemsCategoryEnum.NameEffects,
                    itemType: ShopItemTypeEnum.VisualEffect,
                    price: 500,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 4,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"effect\": \"glow\", \"color\": \"#00AFFF\"}"
                ),
                ShopItem.Create(
                    id: 501,
                    name: "Inferno Name Aura",
                    description: "Your username ignites with flame animation.",
                    imageUrl: "nameeffects/inferno",
                    category: ShopItemsCategoryEnum.NameEffects,
                    itemType: ShopItemTypeEnum.VisualEffect,
                    price: 950,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 10,
                    isPurchasable: true,
                    isUnique: true,
                    effectDataJson: "{\"effect\": \"aura\", \"style\": \"fire\"}"
                ),
                // -------------------------
                // CONSUMABLES
                // -------------------------
                ShopItem.Create(
                    id: 600,
                    name: "Mini XP Snack",
                    description: "+40 XP on next completed task. Tastes digital.",
                    imageUrl: "consumables/snack",
                    category: ShopItemsCategoryEnum.Consumables,
                    itemType: ShopItemTypeEnum.Consumable,
                    price: 120,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 1,
                    isPurchasable: true,
                    isUnique: false,
                    effectDataJson: "{\"type\": \"xp\", \"amount\": 40}"
                ),
                ShopItem.Create(
                    id: 601,
                    name: "Giga XP Meal",
                    description: "+350 XP on next completed task.",
                    imageUrl: "consumables/giga_meal",
                    category: ShopItemsCategoryEnum.Consumables,
                    itemType: ShopItemTypeEnum.Consumable,
                    price: 450,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 6,
                    isPurchasable: true,
                    isUnique: false,
                    effectDataJson: "{\"type\": \"xp\", \"amount\": 350}"
                ),
                ShopItem.Create(
                    id: 602,
                    name: "Ultra Focus Brew",
                    description: "Doubles XP for the next 2 tasks.",
                    imageUrl: "consumables/focus_brew",
                    category: ShopItemsCategoryEnum.Consumables,
                    itemType: ShopItemTypeEnum.Consumable,
                    price: 800,
                    currencyType: CurrencyTypeEnum.Gold,
                    levelRequirement: 10,
                    isPurchasable: true,
                    isUnique: false,
                    effectDataJson: "{\"type\": \"multiplier\", \"x\": 2, \"tasks\": 2}"
                )
            );
        }
    }
}
