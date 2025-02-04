using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.MonthlyQuest
{
    public class UpdateMonthlyQuestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 100 characters.")]
        public required string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(10, ErrorMessage = "Emoji cannot exceed 10 characters.")]
        public string? Emoji { get; set; }

        public string? Priority { get; set; }

        public bool IsCompleted { get; set; }

        [Required(ErrorMessage = "StartDay is required.")]
        [Range(1, 31, ErrorMessage = "StartDay must be between 1 and 31.")]
        public int StartDay { get; set; }

        [Required(ErrorMessage = "EndDay is required.")]
        [Range(1, 31, ErrorMessage = "EndDay must be between 1 and 31.")]
        public int EndDay { get; set; }
    }
}
