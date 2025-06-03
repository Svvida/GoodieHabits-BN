using Domain.Models;

namespace Tests.Factories
{
    public static class QuestLabelFactory
    {
        public static QuestLabel CreateQuestLabel(
            int id = 1,
            int accountId = 1,
            string text = "text")
        {
            return new QuestLabel
            {
                Id = id,
                AccountId = accountId,
                Value = text,
                BackgroundColor = "backgroundColor",
            };
        }
    }
}
