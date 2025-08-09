using Application.Common;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.UserGoals.CreateUserGoal
{
    public class CreateUserGoalCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher) : IRequestHandler<CreateUserGoalCommand, Unit>
    {
        public async Task<Unit> Handle(CreateUserGoalCommand request, CancellationToken cancellationToken)
        {
            GoalTypeEnum goalType = Enum.Parse<GoalTypeEnum>(request.GoalType, true);

            var quest = await unitOfWork.Quests.GetQuestWithAccountAsync(request.QuestId, request.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Quest with ID {request.QuestId} not found.");

            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone];
            DateTime endsAtUtc = CalculateGoalEndTime(goalType, userTimeZone);

            var bonusXp = goalType switch
            {
                GoalTypeEnum.Daily => 5,
                GoalTypeEnum.Weekly => 10,
                GoalTypeEnum.Monthly => 20,
                GoalTypeEnum.Yearly => 50,
                _ => throw new InvalidArgumentException($"Invalid goal type: {goalType}."),
            };

            var userGoal = UserGoal.Create(quest.Id, request.AccountId, goalType, endsAtUtc, bonusXp);

            foreach (var domainEvent in userGoal.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(domainEvent);
                await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }

            userGoal.ClearDomainEvents();

            await unitOfWork.UserGoals.AddAsync(userGoal, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }

        private static DateTime CalculateGoalEndTime(GoalTypeEnum goalType, DateTimeZone userTimeZone)
        {
            Instant nowUtc = SystemClock.Instance.GetCurrentInstant();
            LocalDateTime nowLocal = nowUtc.InZone(userTimeZone).LocalDateTime;

            LocalDateTime endLocal;

            switch (goalType)
            {
                case GoalTypeEnum.Daily:
                    endLocal = nowLocal.Date.PlusDays(1).AtMidnight();
                    break;
                case GoalTypeEnum.Weekly:
                    var daysUntilMonday = (IsoDayOfWeek.Monday - nowLocal.DayOfWeek + 7) % 7;
                    daysUntilMonday = daysUntilMonday == 0 ? 7 : daysUntilMonday; // If today is Monday, set to next Monday
                    endLocal = nowLocal.Date.PlusDays(daysUntilMonday).AtMidnight();
                    break;
                case GoalTypeEnum.Monthly:
                    endLocal = new LocalDateTime(nowLocal.Year, nowLocal.Month, 1, 0, 0, 0)
                        .PlusMonths(1);
                    break;
                case GoalTypeEnum.Yearly:
                    endLocal = new LocalDateTime(nowLocal.Year, 1, 1, 0, 0, 0)
                        .PlusYears(1);
                    break;
                default:
                    throw new InvalidArgumentException($"Invalid goal type: {goalType}.");
            }

            return endLocal.InZoneLeniently(userTimeZone).ToDateTimeUtc();
        }
    }
}
