using Application.Common.Interfaces;
using Application.Quests.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Quests.Commands.UpdateQuest
{
    public abstract record UpdateQuestCommand : ICurrentUserQuestCommand
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; } = null;
        public DateTime? StartDate { get; init; } = null;
        public DateTime? EndDate { get; init; } = null;
        public string? Emoji { get; init; } = null;
        public string? Priority { get; init; } = null;
        public string? Difficulty { get; init; } = null;
        public TimeOnly? ScheduledTime { get; init; } = null;
        public HashSet<int> Labels { get; init; } = [];
        public int QuestId { get; init; }
        public QuestTypeEnum QuestType { get; init; }
        public int AccountId { get; init; }
    }

    public record UpdateOneTimeQuestCommand : UpdateQuestCommand, IRequest<OneTimeQuestDetailsDto>;
    public record UpdateDailyQuestCommand : UpdateQuestCommand, IRequest<DailyQuestDetailsDto>;
    public record UpdateWeeklyQuestCommand : UpdateQuestCommand, IRequest<WeeklyQuestDetailsDto>
    {
        public HashSet<string> Weekdays { get; init; } = [];
    }
    public record UpdateMonthlyQuestCommand : UpdateQuestCommand, IRequest<MonthlyQuestDetailsDto>
    {
        public int StartDay { get; init; }
        public int EndDay { get; init; }
    }
    public record UpdateSeasonalQuestCommand : UpdateQuestCommand, IRequest<SeasonalQuestDetailsDto>
    {
        public string Season { get; init; } = null!;
    }
}
