namespace Application.Dtos.QuestMetadata
{
    public class QuestMetadataDto
    {
        public int Id { get; set; }
        public required string QuestType { get; set; }
        public object? Quest { get; set; }
    }
}
