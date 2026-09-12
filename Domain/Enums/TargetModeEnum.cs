namespace Domain.Enums
{
    /// <summary>Which side of the target counts as success.</summary>
    public enum TargetModeEnum
    {
        /// <summary>Build a habit: the period succeeds once progress reaches the target.</summary>
        AtLeast,

        /// <summary>
        /// Limit a habit ("at most 2 coffees a day"): the period fails as soon as progress exceeds the
        /// target, and succeeds when it elapses without doing so. Reserved — validators reject it until
        /// phase 2, so the enum slot ships without the behaviour needing a migration later.
        /// </summary>
        AtMost
    }
}
