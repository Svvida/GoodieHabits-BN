using System.Text.Json.Serialization;

namespace Application.Dtos.Quests
{
    public abstract class BaseQuestCompletionPatchDto
    {
        [JsonIgnore]
        public int Id { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
