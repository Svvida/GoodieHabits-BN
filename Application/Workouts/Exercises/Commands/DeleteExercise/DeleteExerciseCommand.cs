using Application.Common.Interfaces;

namespace Application.Workouts.Exercises.Commands.DeleteExercise
{
    public record DeleteExerciseCommand(int ExerciseId, int UserProfileId) : ICommand;
}
