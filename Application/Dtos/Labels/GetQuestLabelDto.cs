namespace Application.Dtos.Labels
{
    public class GetQuestLabelDto
    {
        public int Id { get; set; }
        public required string Value { get; set; }
        public required string BackgroundColor { get; set; }
        public required string TextColor { get; set; }
    }
}
