using Domain.Common;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A saved workout template — one runnable session's worth of exercises ("Push A"), which is what
    /// <c>POST /workouts/sessions</c> materializes from.
    /// <para>
    /// A routine is a template and a <see cref="WorkoutSession"/> is a record; the session never writes back.
    /// This is the <see cref="RecurringTransaction"/> → <see cref="FinanceTransaction"/> relationship and it
    /// inherits its rules: editing a routine does not touch past sessions, and deleting one nulls
    /// <c>WorkoutSession.RoutineId</c> rather than deleting the sessions it produced.
    /// </para>
    /// </summary>
    public class WorkoutRoutine : EntityBase
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 500;

        public int Id { get; set; }
        public int UserProfileId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public bool IsArchived { get; private set; }

        public UserProfile UserProfile { get; set; } = null!;
        public ICollection<WorkoutRoutineExercise> Exercises { get; set; } = [];

        protected WorkoutRoutine() { }

        private WorkoutRoutine(int userProfileId, string name, string? description)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            ValidateName(name);
            ValidateDescription(description);

            UserProfileId = userProfileId;
            Name = name.Trim();
            Description = description?.Trim();
        }

        public static WorkoutRoutine Create(int userProfileId, string name, string? description = null)
            => new(userProfileId, name, description);

        public void Rename(string name)
        {
            ValidateName(name);
            Name = name.Trim();
        }

        public void UpdateDescription(string? description)
        {
            ValidateDescription(description);
            Description = description?.Trim();
        }

        public void SetArchived(bool isArchived) => IsArchived = isArchived;

        /// <summary>
        /// Replaces the whole exercise list, renumbering <see cref="WorkoutRoutineExercise.Order"/> from 0 in
        /// the order given. Full replacement rather than partial patching: an ordered collection patched item
        /// by item is a bug farm, and <c>PUT /workouts/routines/{id}</c> is a full-replacement endpoint for the
        /// same reason <c>PUT /finance/transactions/{id}</c> is.
        /// <para>An empty list is legal — a routine under construction is not an error.</para>
        /// </summary>
        public void ReplaceExercises(IEnumerable<WorkoutRoutineExercise> exercises)
        {
            ArgumentNullException.ThrowIfNull(exercises);

            var ordered = exercises.ToList();

            Exercises.Clear();

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].SetOrder(i);
                Exercises.Add(ordered[i]);
            }
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidArgumentException("Routine name cannot be null or whitespace.");
            if (name.Trim().Length > NameMaxLength)
                throw new InvalidArgumentException($"Routine name cannot exceed {NameMaxLength} characters.");
        }

        private static void ValidateDescription(string? description)
        {
            if (description is not null && description.Trim().Length > DescriptionMaxLength)
                throw new InvalidArgumentException($"Description cannot exceed {DescriptionMaxLength} characters.");
        }
    }
}
