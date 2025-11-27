using System.Text.Json.Serialization;
using Domain.Enums;

namespace Domain.ValueObjects
{
    // The Abstract Base
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ConsumablePayload), "Consumable")]
    [JsonDerivedType(typeof(AvatarFramePayload), "AvatarFrame")]
    [JsonDerivedType(typeof(AvatarPayload), "Avatar")]
    [JsonDerivedType(typeof(PetPayload), "Pet")]
    [JsonDerivedType(typeof(TitlePayload), "Title")]
    [JsonDerivedType(typeof(NameEffectPayload), "NameEffect")]
    public abstract class ShopItemPayload
    {
    }

    public class ConsumablePayload : ShopItemPayload
    {
        public ConsumablesEffectTypeEnum EffectType { get; set; }
        public int? DurationMinutes { get; set; } // Null = instant/usage based
        public int? UsageCount { get; set; } // Null = infinite/time based
        public double? Multiplier { get; set; }
        public int? FlatValue { get; set; }
    }

    public class AvatarFramePayload : ShopItemPayload
    {
        public string FrameId { get; set; } = null!;
    }
    public class AvatarPayload : ShopItemPayload
    {
        public string AvatarId { get; set; } = null!; // e.g., "cat_programmer"
    }
    public class PetPayload : ShopItemPayload
    {
        public string PetId { get; set; } = null!;
        public string? Animation { get; set; }
    }
    public class TitlePayload : ShopItemPayload
    {
        public string TitleText { get; set; } = null!; // e.g., "Certified Procrastinator"
    }

    public class NameEffectPayload : ShopItemPayload
    {
        public string EffectStyle { get; set; } = null!; // "glow", "aura"
        public string? ColorHex { get; set; }
    }
}
