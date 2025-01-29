using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.OneTimeQuest
{
    public class UpdateOneTimeQuestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(10, ErrorMessage = "Emoji cannot exceed 10 characters.")]
        public string? Emoji { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        public string? Priority { get; set; }

        [Required(ErrorMessage = "IsCompleted is required.")]
        public bool IsCompleted { get; set; }
    }
}
