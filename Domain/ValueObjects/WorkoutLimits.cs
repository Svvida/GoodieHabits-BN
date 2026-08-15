namespace Domain.ValueObjects
{
    /// <summary>
    /// Bounds shared by everything that carries a training measurement — routine targets and logged sets alike.
    /// One place so the entity guards and the FluentValidation rules can never drift apart, and so the numbers
    /// stay tied to the column precisions in the EF configuration (<c>decimal(6,2)</c> weight,
    /// <c>decimal(8,2)</c> distance).
    /// </summary>
    public static class WorkoutLimits
    {
        public const int MaxSets = 50;
        public const int MaxReps = 1000;
        public const decimal MaxWeight = 9999.99m;
        public const decimal MaxDistance = 999999.99m;
        public const int MaxDurationSeconds = 86_400;   // one day
        public const int MaxRestSeconds = 3_600;        // one hour
        public const decimal MinRpe = 1m;
        public const decimal MaxRpe = 10m;
    }
}
