using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A planned spending/earning limit for a period. A null <see cref="CategoryId"/> represents an
    /// overall budget for the period; a set category scopes it. Overall and per-category budgets may coexist.
    /// </summary>
    public class Budget : EntityBase
    {
        public int Id { get; set; }
        public int UserProfileId { get; private set; }
        public int? CategoryId { get; private set; }   // null => overall budget for the period
        public BudgetPeriodEnum Period { get; private set; }
        public int Year { get; private set; }
        public int? Month { get; private set; }         // 1..12 when Monthly, null when Yearly
        public decimal LimitAmount { get; private set; }

        public UserProfile UserProfile { get; set; } = null!;
        public FinanceCategory? Category { get; set; }

        protected Budget() { }

        private Budget(
            int userProfileId,
            int? categoryId,
            BudgetPeriodEnum period,
            int year,
            int? month,
            decimal limitAmount)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            ValidatePeriod(period, year, month);
            ValidateLimit(limitAmount);

            UserProfileId = userProfileId;
            CategoryId = categoryId;
            Period = period;
            Year = year;
            Month = period == BudgetPeriodEnum.Monthly ? month : null;
            LimitAmount = limitAmount;
        }

        public static Budget Create(
            int userProfileId,
            int? categoryId,
            BudgetPeriodEnum period,
            int year,
            int? month,
            decimal limitAmount)
            => new(userProfileId, categoryId, period, year, month, limitAmount);

        public void UpdateLimit(decimal limitAmount)
        {
            ValidateLimit(limitAmount);
            LimitAmount = limitAmount;
        }

        private static void ValidatePeriod(BudgetPeriodEnum period, int year, int? month)
        {
            if (year is < 1 or > 9999)
                throw new InvalidArgumentException("Year must be between 1 and 9999.");

            if (period == BudgetPeriodEnum.Monthly)
            {
                if (month is null or < 1 or > 12)
                    throw new InvalidArgumentException("Monthly budgets require a month between 1 and 12.");
            }
            else if (month is not null)
            {
                throw new InvalidArgumentException("Yearly budgets must not specify a month.");
            }
        }

        private static void ValidateLimit(decimal limitAmount)
        {
            if (limitAmount <= 0)
                throw new InvalidArgumentException("Budget limit must be greater than zero.");
        }
    }
}
