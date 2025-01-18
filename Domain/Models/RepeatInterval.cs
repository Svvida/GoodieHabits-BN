using Domain.Enum;

namespace Domain.Models
{
    public class RepeatInterval
    {
        public required RepeatIntervalType Type { get; set; }
        public List<string>? Days { get; set; }
    }
}
