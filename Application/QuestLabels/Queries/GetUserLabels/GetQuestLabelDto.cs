namespace Application.QuestLabels.Queries.GetUserLabels
{
    public class GetQuestLabelDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty; // Prevents ASP.NET Core default validation errors.
        public string BackgroundColor { get; set; } = string.Empty;// Prevents ASP.NET Core default validation errors.
    }
}
