using Application.Workouts.Analytics.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Workouts.Analytics.Queries.GetPersonalRecords
{
    public class GetPersonalRecordsQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetPersonalRecordsQuery, IEnumerable<PersonalRecordDto>>
    {
        public async Task<IEnumerable<PersonalRecordDto>> Handle(GetPersonalRecordsQuery request, CancellationToken cancellationToken)
        {
            var records = await unitOfWork.WorkoutSessions
                .GetPersonalRecordsAsync(request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            return records
                .Select(r => new PersonalRecordDto
                {
                    ExerciseId = r.ExerciseId,
                    ExerciseName = r.ExerciseName,
                    MaxWeight = r.MaxWeight,
                    MaxReps = r.MaxReps,
                    MaxSetVolume = r.MaxSetVolume,
                    SetCount = r.SetCount,
                    LastPerformedOn = r.LastPerformedOn,
                })
                .OrderBy(r => r.ExerciseName)
                .ToList();
        }
    }
}
