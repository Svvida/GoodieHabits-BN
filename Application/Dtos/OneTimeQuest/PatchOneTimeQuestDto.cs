using System.ComponentModel.DataAnnotations;
using Domain.Enum;

namespace Application.Dtos.OneTimeQuest
{
    public class PatchOneTimeQuestDto
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 100 characters.")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(10, ErrorMessage = "Emoji cannot exceed 10 characters.")]
        public string? Emoji { get; set; }

        public bool? IsCompleted { get; set; }

        [Range((int)Domain.Enum.Priority.None, (int)Domain.Enum.Priority.High, ErrorMessage = "Priority level is invalid.")]
        public Priority? Priority { get; set; }
    }
}
