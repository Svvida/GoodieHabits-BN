using Application.Supplements.Intakes.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Supplements.Intakes.Common
{
    /// <summary>
    /// Assembles one day's checklist. Shared by the query and by every intake mutation, because they all
    /// answer with the refreshed day — one response the client can repaint the whole screen from.
    /// </summary>
    internal static class ChecklistBuilder
    {
        public static async Task<SupplementChecklistDto> BuildAsync(
            IUnitOfWork unitOfWork,
            int userProfileId,
            DateOnly date,
            IReadOnlyCollection<SupplementTimingEnum>? timings,
            CancellationToken cancellationToken)
        {
            // Inactive supplements are loaded too: their planned doses drop off the list, but an ad-hoc dose
            // still needs a name to render with.
            var supplements = await unitOfWork.Supplements
                .GetUserSupplementsAsync(userProfileId, true, cancellationToken)
                .ConfigureAwait(false);

            var intakes = await unitOfWork.SupplementIntakes
                .GetForDayAsync(userProfileId, date, cancellationToken)
                .ConfigureAwait(false);

            var bySlot = intakes
                .Where(i => i.ScheduleSlotId.HasValue)
                .ToDictionary(i => i.ScheduleSlotId!.Value);

            var items = new List<SupplementChecklistItemDto>();

            foreach (var supplement in supplements.Where(s => s.IsActive))
            {
                foreach (var slot in supplement.Slots)
                {
                    if (timings is { Count: > 0 } && !timings.Contains(slot.Timing))
                        continue;

                    bySlot.TryGetValue(slot.Id, out var intake);
                    items.Add(BuildItem(supplement, slot, intake));
                }
            }

            var catalog = supplements.ToDictionary(s => s.Id);

            var adHoc = intakes
                .Where(i => i.ScheduleSlotId is null)
                .OrderBy(i => i.TakenAt)
                .Select(i => BuildIntake(i, catalog))
                .ToList();

            return new SupplementChecklistDto
            {
                Date = date,
                Items = [.. items.OrderBy(i => i.Timing).ThenBy(i => i.TimeOfDay).ThenBy(i => i.SupplementName)],
                AdHoc = adHoc,
            };
        }

        public static SupplementIntakeDto BuildIntake(SupplementIntake intake, IReadOnlyDictionary<int, Supplement> catalog)
        {
            catalog.TryGetValue(intake.SupplementId, out var supplement);

            return new SupplementIntakeDto
            {
                Id = intake.Id,
                SupplementId = intake.SupplementId,
                SupplementName = supplement?.Name ?? string.Empty,
                Unit = supplement?.Unit ?? SupplementUnitEnum.Piece,
                ScheduleSlotId = intake.ScheduleSlotId,
                TakenOn = intake.TakenOn,
                TakenAt = intake.TakenAt,
                Amount = intake.Amount,
                WorkoutSessionId = intake.WorkoutSessionId,
            };
        }

        private static SupplementChecklistItemDto BuildItem(
            Supplement supplement, SupplementScheduleSlot slot, SupplementIntake? intake) => new()
            {
                SupplementId = supplement.Id,
                SupplementName = supplement.Name,
                Unit = supplement.Unit,
                Color = supplement.Color,
                Icon = supplement.Icon,
                SlotId = slot.Id,
                Timing = slot.Timing,
                TimeOfDay = slot.TimeOfDay,
                OffsetMinutes = slot.OffsetMinutes,
                PlannedAmount = slot.Amount,
                Note = slot.Note,
                Taken = intake is not null,
                IntakeId = intake?.Id,
                TakenAt = intake?.TakenAt,
                TakenAmount = intake?.Amount,
                WorkoutSessionId = intake?.WorkoutSessionId,
            };
    }
}
