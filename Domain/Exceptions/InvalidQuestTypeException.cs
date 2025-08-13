using Domain.Enums;

namespace Domain.Exceptions
{
    public class InvalidQuestTypeException(int questId, QuestTypeEnum expectedType, QuestTypeEnum actualType)
        : AppException($"Quest with ID {questId} is of type {actualType}, but expected {expectedType}.", 400)
    {
        public int QuestId { get; } = questId;
        public QuestTypeEnum ExpectedType { get; } = expectedType;
        public QuestTypeEnum ActualType { get; } = actualType;
    }
}
