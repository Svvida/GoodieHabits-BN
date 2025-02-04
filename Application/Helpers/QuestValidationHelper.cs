using Domain.Exceptions;

namespace Application.Helpers
{
    internal static class QuestValidationHelper
    {
        public static void ValidateWeekdays(List<string> weekdays)
        {
            if (weekdays is null || weekdays.Count == 0)
            {
                throw new InvalidArgumentException("At least one weekday must be selected.");
            }

            if (weekdays.Count != weekdays.Distinct().Count())
            {
                throw new InvalidArgumentException("Weekdays must be unique.");
            }
        }

        public static void ValidateDayRange(int startDay, int endDay)
        {
            if (startDay > endDay)
            {
                throw new InvalidArgumentException($"Start day {startDay} cannot be greater than end day {endDay}.");
            }
        }
    }
}
