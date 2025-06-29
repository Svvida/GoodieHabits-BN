using System.Text.Json.Serialization;

namespace Application.Dtos.Quests
{
    public class QuestCompletionPatchDto
    {
        [JsonIgnore]
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
    }
}
