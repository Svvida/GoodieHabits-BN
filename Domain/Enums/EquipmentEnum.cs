namespace Domain.Enums
{
    /// <summary>
    /// Equipment an exercise needs. <see cref="None"/> at 0 covers bodyweight work and is the safe default for
    /// an omitted field.
    /// <para>
    /// ⚠️ Values are persisted as <c>int</c> (see <c>ExerciseConfiguration</c>), so the numbering is data.
    /// <b>Only ever append</b> new members at the end — inserting one in the middle silently re-labels every
    /// existing row from that point on.
    /// </para>
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
        /// <summary>
        /// Calisthenics/street-workout skill work (muscle-ups, levers, pistols) — its own training category
        /// next to plain <see cref="None"/> bodyweight, because the equipment (bar, parallettes) and the
        /// progression model differ.
        /// </summary>
        Calisthenics,
        /// <summary>Gymnastic rings.</summary>
        Rings,
    }
}
