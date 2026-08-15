using Application.Common.Interfaces;
using Application.Workouts.Analytics.Dtos;

namespace Application.Workouts.Analytics.Queries.GetPersonalRecords
{
    /// <summary>All-time bests per exercise. No date range — records are a fold over the whole history.</summary>
    public record GetPersonalRecordsQuery(int UserProfileId) : IQuery<IEnumerable<PersonalRecordDto>>;
}
