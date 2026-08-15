using Application.Common.Interfaces;
using Application.Supplements.Intakes.Dtos;
using Domain.Enums;

namespace Application.Supplements.Intakes.Queries.GetChecklist
{
    /// <summary>
    /// One day's plan and what has been ticked off. <paramref name="Timings"/> is an optional filter — the
    /// in-training panel passes <c>PreWorkout</c>/<c>PostWorkout</c>, the standalone screen passes nothing.
    /// </summary>
    public record GetChecklistQuery(
        int UserProfileId,
        DateOnly Date,
        IReadOnlyCollection<SupplementTimingEnum>? Timings) : IQuery<SupplementChecklistDto>;
}
