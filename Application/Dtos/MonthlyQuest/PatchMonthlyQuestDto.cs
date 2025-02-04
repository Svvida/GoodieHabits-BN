using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.MonthlyQuest
{
    public class PatchMonthlyQuestDto
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

        [Range(1, 31, ErrorMessage = "StartDay must be between 1 and 31.")]
        public int? StartDay { get; set; }

        [Range(1, 31, ErrorMessage = "EndDay must be between 1 and 31.")]
        public int? EndDay { get; set; }
    }
}
