using Application.Common.Interfaces;
using Application.Workouts.Exercises.Dtos;

namespace Application.Workouts.Exercises.Commands.SetExerciseArchived
{
    public record SetExerciseArchivedCommand(int ExerciseId, bool IsArchived, int UserProfileId)
        : ICommand<ExerciseDto>;
}
