using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.WeeklyQuest
{
    public class PatchWeeklyQuestDto
    {
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(10, ErrorMessage = "Emoji cannot exceed 10 characters.")]
        public string? Emoji { get; set; }

        public bool? IsCompleted { get; set; }

        public string? Priority { get; set; }

        public List<string>? Weekdays { get; set; }
    }
}
