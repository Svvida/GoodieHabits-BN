using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Queries.GetQuests
{
    /// <summary>
    /// All of a user's quests, optionally filtered. Replaces <c>GET /api/quests/{questType}</c>, whose path
    /// segment no longer names anything real.
    /// </summary>
    /// <param name="Unit">"None", "Day", "Week", "Month" or "Year".</param>
    /// <param name="LegacyType">
    /// One of the retired quest types, for the per-type screens that already exist. Deprecated on arrival —
    /// it is a bridge, not the model.
    /// </param>
    public record GetQuestsQuery(int UserProfileId, string? Unit = null, string? LegacyType = null)
        : IQuery<IEnumerable<QuestDetailsDto>>;
}
