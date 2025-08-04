using Domain.Common;
using Domain.Exceptions;

namespace Domain.Models
{
    public class QuestLabel : EntityBase
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Value { get; private set; } = null!;
        public string BackgroundColor { get; private set; } = null!;

        public Account Account { get; set; } = null!;
        public ICollection<Quest_QuestLabel> Quest_QuestLabels { get; set; } = [];

        protected QuestLabel() { }
        private QuestLabel(int accountId, string value, string backgroundColor)
        {
            if (accountId <= 0)
                throw new InvalidArgumentException("AccountId must be greater than zero.");

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidArgumentException("Value cannot be null or whitespace.");

            if (string.IsNullOrWhiteSpace(backgroundColor) || backgroundColor.Length != 7)
                throw new InvalidArgumentException("BackgroundColor must be a valid hex color code.");

            AccountId = accountId;
            Value = value;
            BackgroundColor = backgroundColor;
        }

        public static QuestLabel Create(
            int accountId,
            string value,
            string backgroundColor)
        {
            return new QuestLabel(accountId, value, backgroundColor);
        }

        public void UpdateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidArgumentException("Value cannot be null or whitespace.");
            Value = value;
        }

        public void UpdateBackgroundColor(string? backgroundColor)
        {
            if (string.IsNullOrWhiteSpace(backgroundColor) || backgroundColor.Length != 7)
                throw new InvalidArgumentException("BackgroundColor must be a valid hex color code.");
            BackgroundColor = backgroundColor;
        }
    }
}