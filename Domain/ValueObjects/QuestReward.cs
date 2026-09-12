namespace Domain.ValueObjects
{
    /// <summary>What completing one quest period pays out. Recorded on the period once it is granted.</summary>
    public readonly record struct QuestReward(int Xp, int Coins)
    {
        public static QuestReward None { get; } = new(0, 0);

        public bool IsNothing => Xp == 0 && Coins == 0;
    }
}
