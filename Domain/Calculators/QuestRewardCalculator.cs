using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// What reaching a period's target is worth.
    /// <para>
    /// Rewards are paid <b>per period</b>, not per tap. Paying per tap would make a "10 glasses of water"
    /// habit earn ten times what a daily gym habit earns, purely because its target is counted in smaller
    /// units. The period is the unit of accountability, so it is also the unit of payment — and because the
    /// payment is recorded on the period and granted once, the old complete/uncomplete coin farm is closed
    /// by construction rather than by a guard.
    /// </para>
    /// <para>
    /// Balance is preserved for the common case on purpose: a Day-unit quest with no modifiers still pays
    /// 10 XP and 10 coins, exactly as before.
    /// </para>
    /// </summary>
    public static class QuestRewardCalculator
    {
        public const int BaseXp = 10;
        public const int BaseCoins = 10;

        /// <summary>
        /// How much more one period of a longer unit is worth. A weekly target is a bigger commitment than a
        /// daily one, but not seven times bigger — most of the effort of a habit is remembering it at all.
        /// </summary>
        public static int PeriodWeight(PeriodUnitEnum unit) => unit switch
        {
            PeriodUnitEnum.Day => 1,
            PeriodUnitEnum.Week => 3,
            PeriodUnitEnum.Month => 8,
            PeriodUnitEnum.Year => 20,
            // A one-time quest keeps the flat value it had before this change.
            PeriodUnitEnum.None => 1,
            _ => 1
        };

        public static QuestReward Calculate(
            QuestSchedule schedule,
            DifficultyEnum? difficulty,
            PriorityEnum? priority,
            DateOnly? endDate,
            DateOnly completedOn)
        {
            int weight = PeriodWeight(schedule.Unit);
            int xp = (BaseXp + DifficultyBonus(difficulty) + PriorityBonus(priority) + OnTimeBonus(schedule, endDate, completedOn)) * weight;

            return new QuestReward(xp, BaseCoins * weight);
        }

        private static int DifficultyBonus(DifficultyEnum? difficulty) => difficulty switch
        {
            DifficultyEnum.Easy => 0,
            DifficultyEnum.Medium => 5,
            DifficultyEnum.Hard => 10,
            DifficultyEnum.Impossible => 20,
            _ => 0
        };

        private static int PriorityBonus(PriorityEnum? priority) => priority switch
        {
            PriorityEnum.Low => 0,
            PriorityEnum.Medium => 5,
            PriorityEnum.High => 10,
            _ => 0
        };

        /// <summary>
        /// A deadline bonus only means something for a quest that has a deadline. The old rule paid it to
        /// every repeatable quest with an end date, on every completion, because "today is on or before the
        /// end date" is true for a repeating quest's entire life — so it was a flat 5 XP wearing a disguise.
        /// </summary>
        private static int OnTimeBonus(QuestSchedule schedule, DateOnly? endDate, DateOnly completedOn)
        {
            if (schedule.IsRepeatable || endDate is not DateOnly deadline)
                return 0;

            return completedOn <= deadline ? 5 : 0;
        }
    }
}
