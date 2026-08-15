namespace Domain.Enums
{
    /// <summary>
    /// Equipment an exercise needs. <see cref="None"/> at 0 covers bodyweight work and is the safe default for
    /// an omitted field.
    /// </summary>
    public enum EquipmentEnum
    {
        /// <summary>Bodyweight — no equipment at all.</summary>
        None,
        Barbell,
        Dumbbell,
        Kettlebell,
        Machine,
        Cable,
        ResistanceBand,
        Other,
    }
}
