using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.ToTable("Exercises");
            builder.HasKey(e => e.Id);

            // The library read is always "system rows + mine", so the owner leads every index.
            builder.HasIndex(e => e.UserProfileId);
            builder.HasIndex(e => new { e.UserProfileId, e.MuscleGroup });

            builder.Property(e => e.Name)
                .HasMaxLength(Exercise.NameMaxLength)
                .IsRequired();

            builder.Property(e => e.MetricType)
                .IsRequired();

            builder.Property(e => e.MuscleGroup)
                .IsRequired();

            builder.Property(e => e.Equipment)
                .IsRequired();

            builder.Property(e => e.Note)
                .HasMaxLength(Exercise.NoteMaxLength);

            builder.Property(e => e.IsSystem)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(e => e.IsArchived)
                .HasDefaultValue(false)
                .IsRequired();

            // Null owner => a seeded system exercise shared by everyone, exactly like FinanceCategory.
            builder.HasOne(e => e.UserProfile)
                .WithMany(u => u.Exercises)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            SeedSystemExercises(builder);
        }

        /// <summary>
        /// The starter library. Names are Polish display strings — there is no exercise i18n on the backend,
        /// the same call the finance taxonomy made. The bias is deliberate: <b>calisthenics first</b>, with
        /// enough barbell/dumbbell/machine work that a gym session can be logged without inventing rows.
        /// The list is a starting point, not a canon — users add their own freely, and archiving hides what
        /// they don't do.
        /// <para>
        /// Ids are grouped by muscle block and <b>must stay stable across migrations</b>:
        /// chest 1-99, back 100-199, shoulders 200-299, arms 300-399, core 400-499, legs 500-599,
        /// full body 600-699, cardio 700-799. Number new rows inside the matching block.
        /// </para>
        /// <para>
        /// ⚠️ These ids share one IDENTITY sequence with user-created exercises. The table's first migration
        /// reseeds that sequence to <b>100 000</b> — system rows below, user rows above — because
        /// <c>HasData</c> raises the identity counter without reserving a range, which is exactly how a seed
        /// migration collided with live user rows in <c>FinanceCategories</c> on 2026-08-09. Never seed an id
        /// at or above 100 000.
        /// </para>
        /// </summary>
        private static void SeedSystemExercises(EntityTypeBuilder<Exercise> builder)
        {
            const ExerciseMetricEnum reps = ExerciseMetricEnum.Reps;
            const ExerciseMetricEnum load = ExerciseMetricEnum.RepsAndWeight;
            const ExerciseMetricEnum time = ExerciseMetricEnum.Time;
            const ExerciseMetricEnum dist = ExerciseMetricEnum.Distance;
            const ExerciseMetricEnum distTime = ExerciseMetricEnum.DistanceAndTime;

            const EquipmentEnum body = EquipmentEnum.None;
            const EquipmentEnum bar = EquipmentEnum.Barbell;
            const EquipmentEnum db = EquipmentEnum.Dumbbell;
            const EquipmentEnum kb = EquipmentEnum.Kettlebell;
            const EquipmentEnum machine = EquipmentEnum.Machine;
            const EquipmentEnum cable = EquipmentEnum.Cable;
            const EquipmentEnum band = EquipmentEnum.ResistanceBand;
            const EquipmentEnum other = EquipmentEnum.Other;

            static Exercise Ex(int id, string name, ExerciseMetricEnum metric, MuscleGroupEnum muscle, EquipmentEnum equipment)
                => Exercise.CreateSystem(id, name, metric, muscle, equipment);

            builder.HasData(
                // ── Klatka piersiowa (1-99) ─────────────────────────────────────────────────────────────
                Ex(1, "Pompki klasyczne", reps, MuscleGroupEnum.Chest, body),
                Ex(2, "Pompki szerokie", reps, MuscleGroupEnum.Chest, body),
                Ex(3, "Pompki diamentowe", reps, MuscleGroupEnum.Chest, body),
                Ex(4, "Pompki z nogami na podwyższeniu", reps, MuscleGroupEnum.Chest, body),
                Ex(5, "Pompki na podwyższeniu (łatwiejsze)", reps, MuscleGroupEnum.Chest, body),
                Ex(6, "Pompki archer", reps, MuscleGroupEnum.Chest, body),
                Ex(7, "Pompki wybuchowe (z klaśnięciem)", reps, MuscleGroupEnum.Chest, body),
                Ex(8, "Pompki na jednej ręce", reps, MuscleGroupEnum.Chest, body),
                Ex(9, "Dipy na poręczach", reps, MuscleGroupEnum.Chest, body),
                Ex(10, "Pompki na kółkach gimnastycznych", reps, MuscleGroupEnum.Chest, other),
                Ex(11, "Wyciskanie sztangi leżąc", load, MuscleGroupEnum.Chest, bar),
                Ex(12, "Wyciskanie sztangi skos góra", load, MuscleGroupEnum.Chest, bar),
                Ex(13, "Wyciskanie hantli leżąc", load, MuscleGroupEnum.Chest, db),
                Ex(14, "Rozpiętki hantlami", load, MuscleGroupEnum.Chest, db),
                Ex(15, "Rozpiętki na bramie", load, MuscleGroupEnum.Chest, cable),
                Ex(16, "Wyciskanie na maszynie", load, MuscleGroupEnum.Chest, machine),

                // ── Plecy (100-199) ─────────────────────────────────────────────────────────────────────
                Ex(100, "Podciąganie nachwytem", reps, MuscleGroupEnum.Back, body),
                Ex(101, "Podciąganie podchwytem", reps, MuscleGroupEnum.Back, body),
                Ex(102, "Podciąganie chwytem neutralnym", reps, MuscleGroupEnum.Back, body),
                Ex(103, "Podciąganie szerokim chwytem", reps, MuscleGroupEnum.Back, body),
                Ex(104, "Podciąganie australijskie", reps, MuscleGroupEnum.Back, body),
                Ex(105, "Podciąganie z gumą", reps, MuscleGroupEnum.Back, band),
                Ex(106, "Podciąganie łopatkowe", reps, MuscleGroupEnum.Back, body),
                Ex(107, "Muscle-up", reps, MuscleGroupEnum.Back, body),
                Ex(108, "Front lever (wytrzymanie)", time, MuscleGroupEnum.Back, body),
                Ex(109, "Zwis na drążku", time, MuscleGroupEnum.Back, body),
                Ex(110, "Superman (wytrzymanie)", time, MuscleGroupEnum.Back, body),
                Ex(111, "Wiosłowanie sztangą", load, MuscleGroupEnum.Back, bar),
                Ex(112, "Wiosłowanie hantlem", load, MuscleGroupEnum.Back, db),
                Ex(113, "Ściąganie drążka wyciągu górnego", load, MuscleGroupEnum.Back, cable),
                Ex(114, "Wiosłowanie na wyciągu siedząc", load, MuscleGroupEnum.Back, cable),
                Ex(115, "Martwy ciąg", load, MuscleGroupEnum.Back, bar),

                // ── Barki (200-299) ─────────────────────────────────────────────────────────────────────
                Ex(200, "Pike push-ups", reps, MuscleGroupEnum.Shoulders, body),
                Ex(201, "Pompki w staniu na rękach", reps, MuscleGroupEnum.Shoulders, body),
                Ex(202, "Stanie na rękach (wytrzymanie)", time, MuscleGroupEnum.Shoulders, body),
                Ex(203, "Wyciskanie żołnierskie", load, MuscleGroupEnum.Shoulders, bar),
                Ex(204, "Wyciskanie hantli nad głowę", load, MuscleGroupEnum.Shoulders, db),
                Ex(205, "Wznosy bokiem", load, MuscleGroupEnum.Shoulders, db),
                Ex(206, "Wznosy w opadzie tułowia", load, MuscleGroupEnum.Shoulders, db),
                Ex(207, "Face pull", load, MuscleGroupEnum.Shoulders, cable),
                Ex(208, "Krążenia ramion z gumą", reps, MuscleGroupEnum.Shoulders, band),

                // ── Ramiona (300-399) ───────────────────────────────────────────────────────────────────
                Ex(300, "Dipy na ławce (triceps)", reps, MuscleGroupEnum.Triceps, body),
                Ex(301, "Pompki francuskie na drążku", reps, MuscleGroupEnum.Triceps, body),
                Ex(302, "Wyciskanie francuskie", load, MuscleGroupEnum.Triceps, bar),
                Ex(303, "Prostowanie ramion na wyciągu", load, MuscleGroupEnum.Triceps, cable),
                Ex(304, "Podciąganie podchwytem wąsko", reps, MuscleGroupEnum.Biceps, body),
                Ex(305, "Uginanie ramion ze sztangą", load, MuscleGroupEnum.Biceps, bar),
                Ex(306, "Uginanie ramion z hantlami", load, MuscleGroupEnum.Biceps, db),
                Ex(307, "Uginanie młotkowe", load, MuscleGroupEnum.Biceps, db),
                Ex(308, "Uginanie ramion z gumą", reps, MuscleGroupEnum.Biceps, band),
                Ex(309, "Zwis na ręczniku (chwyt)", time, MuscleGroupEnum.Forearms, body),
                Ex(310, "Uginanie nadgarstków", load, MuscleGroupEnum.Forearms, bar),
                Ex(311, "Spacer farmera", dist, MuscleGroupEnum.Forearms, db),

                // ── Core (400-499) ──────────────────────────────────────────────────────────────────────
                Ex(400, "Plank (deska)", time, MuscleGroupEnum.Abs, body),
                Ex(401, "Plank bokiem", time, MuscleGroupEnum.Abs, body),
                Ex(402, "Hollow body hold", time, MuscleGroupEnum.Abs, body),
                Ex(403, "L-sit (wytrzymanie)", time, MuscleGroupEnum.Abs, body),
                Ex(404, "Brzuszki", reps, MuscleGroupEnum.Abs, body),
                Ex(405, "Wznosy nóg leżąc", reps, MuscleGroupEnum.Abs, body),
                Ex(406, "Unoszenie kolan w zwisie", reps, MuscleGroupEnum.Abs, body),
                Ex(407, "Unoszenie nóg w zwisie", reps, MuscleGroupEnum.Abs, body),
                Ex(408, "Toes to bar", reps, MuscleGroupEnum.Abs, body),
                Ex(409, "Rowerek", reps, MuscleGroupEnum.Abs, body),
                Ex(410, "Russian twist", reps, MuscleGroupEnum.Abs, body),
                Ex(411, "Mountain climbers", reps, MuscleGroupEnum.Abs, body),
                Ex(412, "Dragon flag", reps, MuscleGroupEnum.Abs, body),
                Ex(413, "Ab wheel (kółko)", reps, MuscleGroupEnum.Abs, other),

                // ── Nogi (500-599) ──────────────────────────────────────────────────────────────────────
                Ex(500, "Przysiady", reps, MuscleGroupEnum.Quadriceps, body),
                Ex(501, "Przysiady bułgarskie", reps, MuscleGroupEnum.Quadriceps, body),
                Ex(502, "Przysiad pistolet", reps, MuscleGroupEnum.Quadriceps, body),
                Ex(503, "Wykroki", reps, MuscleGroupEnum.Quadriceps, body),
                Ex(504, "Wchodzenie na podwyższenie", reps, MuscleGroupEnum.Quadriceps, body),
                Ex(505, "Przysiad przy ścianie (wytrzymanie)", time, MuscleGroupEnum.Quadriceps, body),
                Ex(506, "Wyskoki z przysiadu", reps, MuscleGroupEnum.Quadriceps, body),
                Ex(507, "Przysiad ze sztangą", load, MuscleGroupEnum.Quadriceps, bar),
                Ex(508, "Przysiad przedni", load, MuscleGroupEnum.Quadriceps, bar),
                Ex(509, "Wyciskanie nogami na maszynie", load, MuscleGroupEnum.Quadriceps, machine),
                Ex(510, "Prostowanie nóg na maszynie", load, MuscleGroupEnum.Quadriceps, machine),
                Ex(511, "Nordic curl", reps, MuscleGroupEnum.Hamstrings, body),
                Ex(512, "Uginanie nóg leżąc", load, MuscleGroupEnum.Hamstrings, machine),
                Ex(513, "Martwy ciąg na prostych nogach", load, MuscleGroupEnum.Hamstrings, bar),
                Ex(514, "Mostek biodrowy", reps, MuscleGroupEnum.Glutes, body),
                Ex(515, "Hip thrust", load, MuscleGroupEnum.Glutes, bar),
                Ex(516, "Wspięcia na palce", reps, MuscleGroupEnum.Calves, body),
                Ex(517, "Wspięcia na palce ze sztangą", load, MuscleGroupEnum.Calves, bar),

                // ── Całe ciało (600-699) ────────────────────────────────────────────────────────────────
                Ex(600, "Burpees", reps, MuscleGroupEnum.FullBody, body),
                Ex(601, "Bear crawl", dist, MuscleGroupEnum.FullBody, body),
                Ex(602, "Swing kettlebell", load, MuscleGroupEnum.FullBody, kb),
                Ex(603, "Turkish get-up", load, MuscleGroupEnum.FullBody, kb),
                Ex(604, "Thruster", load, MuscleGroupEnum.FullBody, bar),
                Ex(605, "Rozgrzewka", time, MuscleGroupEnum.FullBody, body),
                Ex(606, "Rozciąganie", time, MuscleGroupEnum.FullBody, body),

                // ── Cardio (700-799) ────────────────────────────────────────────────────────────────────
                Ex(700, "Bieg", distTime, MuscleGroupEnum.Cardio, body),
                Ex(701, "Spacer / marsz", distTime, MuscleGroupEnum.Cardio, body),
                Ex(702, "Rower", distTime, MuscleGroupEnum.Cardio, other),
                Ex(703, "Bieżnia", distTime, MuscleGroupEnum.Cardio, machine),
                Ex(704, "Orbitrek", time, MuscleGroupEnum.Cardio, machine),
                Ex(705, "Wioślarz", distTime, MuscleGroupEnum.Cardio, machine),
                Ex(706, "Skakanka", time, MuscleGroupEnum.Cardio, other),
                Ex(707, "Pływanie", distTime, MuscleGroupEnum.Cardio, body),
                Ex(708, "Interwały (HIIT)", time, MuscleGroupEnum.Cardio, body));
        }
    }
}
