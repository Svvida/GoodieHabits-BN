using Domain.Enum;

namespace Domain.Exceptions
{
    public class InvalidQuestTypeException : AppException
    {
        public InvalidQuestTypeException(int questId, QuestTypeEnum expectedType, QuestTypeEnum actualType)
            : base($"Quest with ID {questId} is of type {actualType}, but expected {expectedType}.", 400)
        {
            QuestId = questId;
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public int QuestId { get; }
        public QuestTypeEnum ExpectedType { get; }
        public QuestTypeEnum ActualType { get; }
    }
}
