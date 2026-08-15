using Domain.Enums;

namespace Application.Supplements.Catalog.Commands.AddSupplementSlot
{
    /// <summary>
    /// A planned dose. Shared by the add and update endpoints — a slot is small enough that partial patching
    /// would buy nothing but ambiguity.
    /// <para>
    /// <c>Custom</c> timing requires <see cref="TimeOfDay"/>. <see cref="OffsetMinutes"/> is signed and only
    /// meaningful for <c>PreWorkout</c> / <c>PostWorkout</c>: <c>-30</c> reads as "30 min przed treningiem".
    /// The amount is in the parent supplement's unit.
    /// </para>
    /// </summary>
    public record SupplementSlotRequest(
        SupplementTimingEnum Timing,
        decimal Amount,
        TimeOnly? TimeOfDay = null,
        int? OffsetMinutes = null,
        string? Note = null);
}
