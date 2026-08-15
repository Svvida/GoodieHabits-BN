using Application.Workouts.Exercises.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Exercises.Queries.GetExercises
{
    public class GetExercisesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetExercisesQuery, IEnumerable<ExerciseDto>>
    {
        public async Task<IEnumerable<ExerciseDto>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
        {
            var exercises = await unitOfWork.Exercises
                .GetVisibleAsync(
                    request.UserProfileId,
                    request.MuscleGroup,
                    request.MetricType,
                    request.Search,
                    request.IncludeArchived,
                    cancellationToken)
                .ConfigureAwait(false);

            return exercises.Select(mapper.Map<ExerciseDto>).ToList();
        }
    }
}
