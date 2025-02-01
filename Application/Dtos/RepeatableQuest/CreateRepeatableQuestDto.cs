using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.RepeatableQuest
{
    public class CreateRepeatableQuestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; } = null;

        public DateTime? StartDate { get; set; } = null;

        public DateTime? EndDate { get; set; } = null;

        [StringLength(10, ErrorMessage = "Emoji cannot exceed 10 characters.")]
        public string? Emoji { get; set; } = null;

        public string? Priority { get; set; } = null;

        [Required(ErrorMessage = "Repeat interval is required.")]
        public RepeatIntervalDto RepeatInterval { get; set; } = null!;
        public bool IsCompleted { get; set; } = false;
        public int AccountId { get; set; } = 1;
    }
}
