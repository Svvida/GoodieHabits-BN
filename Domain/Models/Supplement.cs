using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A supplement the user takes — the "what". The "when and how much" lives in
    /// <see cref="SupplementScheduleSlot"/>: magnesium morning <em>and</em> evening is one supplement with two
    /// slots, not two supplements.
    /// <para>
    /// The module is deliberately independent of workouts. A supplement plan has to work on a rest day, so
    /// nothing here hangs off a training session; the in-training panel is just the checklist filtered by
    /// <see cref="SupplementTimingEnum.PreWorkout"/> / <see cref="SupplementTimingEnum.PostWorkout"/>.
    /// </para>
    /// </summary>
    public class Supplement : EntityBase
    {
        public const int NameMaxLength = 100;
        public const int NoteMaxLength = 500;
        public const int IconMaxLength = 50;

        public int Id { get; set; }
        public int UserProfileId { get; private set; }
        public string Name { get; private set; } = null!;

        /// <summary>
        /// The dosing unit for this supplement and every one of its slots. Slots carry only an amount — a
        /// supplement whose slots disagreed about the unit would make every aggregate over its intakes
        /// meaningless. Same inheritance call as a finance sub-category inheriting its parent's type.
        /// </summary>
        public SupplementUnitEnum Unit { get; private set; }

        /// <summary>Fallback amount for an ad-hoc intake that names no slot.</summary>
        public decimal? DefaultAmount { get; private set; }

        public string? Note { get; private set; }
        public string? Color { get; private set; }

        /// <summary>Ionicons outline name, matching the finance category convention.</summary>
        public string? Icon { get; private set; }

        /// <summary>
        /// Retire path. Deleting a supplement is blocked once it has intakes, because that would erase history;
        /// deactivating hides it from the checklist and keeps every past row intact.
        /// </summary>
        public bool IsActive { get; private set; } = true;

        public UserProfile UserProfile { get; set; } = null!;
        public ICollection<SupplementScheduleSlot> Slots { get; set; } = [];
        public ICollection<SupplementIntake> Intakes { get; set; } = [];

        protected Supplement() { }

        private Supplement(
            int userProfileId,
            string name,
            SupplementUnitEnum unit,
            decimal? defaultAmount,
            string? note,
            string? color,
            string? icon)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            ValidateName(name);
            ValidateAmount(defaultAmount);
            ValidateNote(note);
            ValidateColor(color);
            ValidateIcon(icon);

            UserProfileId = userProfileId;
            Name = name.Trim();
            Unit = unit;
            DefaultAmount = defaultAmount;
            Note = note?.Trim();
            Color = color;
            Icon = icon;
        }

        public static Supplement Create(
            int userProfileId,
            string name,
            SupplementUnitEnum unit,
            decimal? defaultAmount = null,
            string? note = null,
            string? color = null,
            string? icon = null)
            => new(userProfileId, name, unit, defaultAmount, note, color, icon);

        public void Rename(string name)
        {
            ValidateName(name);
            Name = name.Trim();
        }

        public void UpdateUnit(SupplementUnitEnum unit) => Unit = unit;

        public void UpdateDefaultAmount(decimal? defaultAmount)
        {
            ValidateAmount(defaultAmount);
            DefaultAmount = defaultAmount;
        }

        public void UpdateNote(string? note)
        {
            ValidateNote(note);
            Note = note?.Trim();
        }

        public void UpdateAppearance(string? color, string? icon)
        {
            ValidateColor(color);
            ValidateIcon(icon);
            Color = color;
            Icon = icon;
        }

        public void SetActive(bool isActive) => IsActive = isActive;

        public SupplementScheduleSlot AddSlot(
            SupplementTimingEnum timing,
            decimal amount,
            TimeOnly? timeOfDay = null,
            int? offsetMinutes = null,
            string? note = null)
        {
            var slot = SupplementScheduleSlot.Create(timing, amount, timeOfDay, offsetMinutes, note);
            slot.AttachTo(Id);
            Slots.Add(slot);
            return slot;
        }

        public void RemoveSlot(SupplementScheduleSlot slot)
        {
            ArgumentNullException.ThrowIfNull(slot);

            if (!Slots.Remove(slot))
                throw new NotFoundException("Slot does not belong to this supplement.");
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidArgumentException("Supplement name cannot be null or whitespace.");
            if (name.Trim().Length > NameMaxLength)
                throw new InvalidArgumentException($"Supplement name cannot exceed {NameMaxLength} characters.");
        }

        private static void ValidateAmount(decimal? amount)
        {
            if (amount is decimal value && value <= 0)
                throw new InvalidArgumentException("Amount must be greater than zero.");
        }

        private static void ValidateNote(string? note)
        {
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");
        }

        private static void ValidateColor(string? color)
        {
            if (color is null)
                return;
            if (color.Length != 7 || color[0] != '#')
                throw new InvalidArgumentException("Color must be a valid hex color code (e.g. #RRGGBB).");
        }

        private static void ValidateIcon(string? icon)
        {
            if (icon is not null && icon.Trim().Length > IconMaxLength)
                throw new InvalidArgumentException($"Icon cannot exceed {IconMaxLength} characters.");
        }
    }
}
