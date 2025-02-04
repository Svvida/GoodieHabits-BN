using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.SeasonalQuest
{
    public class PatchSeasonalQuestDto
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

        public string? Priority { get; set; }

        public string? Season { get; set; }
    }
}
