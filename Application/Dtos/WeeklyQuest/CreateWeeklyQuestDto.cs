using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.WeeklyQuest
{
    public class CreateWeeklyQuestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 100 characters.")]
        public required string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; } = null;

        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;

        [StringLength(10, ErrorMessage = "Emoji cannot exceed 10 characters.")]
        public string? Emoji { get; set; } = null;

        public string? Priority { get; set; } = null;

        public int AccountId { get; set; } = 1;
        [Required(ErrorMessage = "Weekdays is required.")]
        [MinLength(1, ErrorMessage = "At least one weekday must be selected.")]
        public List<string> Weekdays { get; set; } = new();
    }
}
