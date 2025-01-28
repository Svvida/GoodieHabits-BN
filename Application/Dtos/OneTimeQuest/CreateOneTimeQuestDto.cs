using System.ComponentModel.DataAnnotations;
using Domain.Enum;

namespace Application.Dtos.OneTimeQuest
{
    public class CreateOneTimeQuestDto
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

        [Range((int)Priority.None, (int)Priority.High, ErrorMessage = "Priority level is invalid.")]
        public Priority Priority { get; set; } = Priority.None;

        public int AccountId { get; set; } = 1;
    }
}