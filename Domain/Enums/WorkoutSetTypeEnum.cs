namespace Domain.Enums
{
    /// <summary>
    /// What kind of set was performed.
    /// <para>
    /// ⚠️ <see cref="DropSet"/> and <see cref="FailureSet"/> are <em>reserved slots</em> for the supersets /
    /// advanced-technique backlog item and are not produced or consumed anywhere in v1 — the same posture as
    /// <c>FinanceTransactionTypeEnum.Transfer</c>. Do not repurpose them.
    /// </para>
    /// </summary>
    public enum WorkoutSetTypeEnum
    {
        /// <summary>A working set. Counts toward volume and personal records.</summary>
        Normal,

        /// <summary>A warm-up set. Excluded from volume and personal records by default.</summary>
        Warmup,

        /// <summary>Reserved — not used in v1.</summary>
        DropSet,

        /// <summary>Reserved — not used in v1.</summary>
        FailureSet,
    }
}
