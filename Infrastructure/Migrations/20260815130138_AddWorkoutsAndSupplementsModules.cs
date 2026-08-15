using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutsAndSupplementsModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeightUnit",
                table: "UserProfiles",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "kg");

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricType = table.Column<int>(type: "int", nullable: false),
                    MuscleGroup = table.Column<int>(type: "int", nullable: false),
                    Equipment = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Supplements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    DefaultAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supplements_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkoutRoutines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutRoutines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutRoutines_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplementScheduleSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplementId = table.Column<int>(type: "int", nullable: false),
                    Timing = table.Column<int>(type: "int", nullable: false),
                    TimeOfDay = table.Column<TimeOnly>(type: "time", nullable: true),
                    OffsetMinutes = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementScheduleSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplementScheduleSlots_Supplements_SupplementId",
                        column: x => x.SupplementId,
                        principalTable: "Supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutRoutineExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutRoutineId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TargetSets = table.Column<int>(type: "int", nullable: true),
                    TargetReps = table.Column<int>(type: "int", nullable: true),
                    TargetWeight = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    TargetDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    TargetDistance = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    RestSeconds = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutRoutineExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutRoutineExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutRoutineExercises_WorkoutRoutines_WorkoutRoutineId",
                        column: x => x.WorkoutRoutineId,
                        principalTable: "WorkoutRoutines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    RoutineId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PerformedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_WorkoutRoutines_RoutineId",
                        column: x => x.RoutineId,
                        principalTable: "WorkoutRoutines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplementIntakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    SupplementId = table.Column<int>(type: "int", nullable: false),
                    ScheduleSlotId = table.Column<int>(type: "int", nullable: true),
                    TakenOn = table.Column<DateOnly>(type: "date", nullable: false),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WorkoutSessionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementIntakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplementIntakes_SupplementScheduleSlots_ScheduleSlotId",
                        column: x => x.ScheduleSlotId,
                        principalTable: "SupplementScheduleSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplementIntakes_Supplements_SupplementId",
                        column: x => x.SupplementId,
                        principalTable: "Supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplementIntakes_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplementIntakes_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessionExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutSessionId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: true),
                    ExerciseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricType = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TargetSets = table.Column<int>(type: "int", nullable: true),
                    TargetReps = table.Column<int>(type: "int", nullable: true),
                    TargetWeight = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    TargetDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    TargetDistance = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    RestSeconds = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessionExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionExercises_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutSessionExerciseId = table.Column<int>(type: "int", nullable: false),
                    SetNumber = table.Column<int>(type: "int", nullable: false),
                    Reps = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    Distance = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Rpe = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: true),
                    SetType = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSets_WorkoutSessionExercises_WorkoutSessionExerciseId",
                        column: x => x.WorkoutSessionExerciseId,
                        principalTable: "WorkoutSessionExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_UserProfileId",
                table: "Exercises",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_UserProfileId_MuscleGroup",
                table: "Exercises",
                columns: new[] { "UserProfileId", "MuscleGroup" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplementIntakes_ScheduleSlotId_TakenOn",
                table: "SupplementIntakes",
                columns: new[] { "ScheduleSlotId", "TakenOn" },
                unique: true,
                filter: "[ScheduleSlotId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SupplementIntakes_SupplementId",
                table: "SupplementIntakes",
                column: "SupplementId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplementIntakes_UserProfileId_TakenOn",
                table: "SupplementIntakes",
                columns: new[] { "UserProfileId", "TakenOn" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplementIntakes_WorkoutSessionId",
                table: "SupplementIntakes",
                column: "WorkoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplements_UserProfileId",
                table: "Supplements",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplementScheduleSlots_SupplementId",
                table: "SupplementScheduleSlots",
                column: "SupplementId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutRoutineExercises_ExerciseId",
                table: "WorkoutRoutineExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutRoutineExercises_WorkoutRoutineId_Order",
                table: "WorkoutRoutineExercises",
                columns: new[] { "WorkoutRoutineId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutRoutines_UserProfileId",
                table: "WorkoutRoutines",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_ExerciseId",
                table: "WorkoutSessionExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_Order",
                table: "WorkoutSessionExercises",
                columns: new[] { "WorkoutSessionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_RoutineId",
                table: "WorkoutSessions",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_UserProfileId_ActiveOnly",
                table: "WorkoutSessions",
                column: "UserProfileId",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_UserProfileId_PerformedOn",
                table: "WorkoutSessions",
                columns: new[] { "UserProfileId", "PerformedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSets_WorkoutSessionExerciseId_SetNumber",
                table: "WorkoutSets",
                columns: new[] { "WorkoutSessionExerciseId", "SetNumber" });

            // Exercises mixes seeded system rows (HasData, explicit ids) with user-created rows on one IDENTITY
            // sequence — the arrangement that broke PK_FinanceCategories in production on 2026-08-09. HasData
            // writes explicit ids under IDENTITY_INSERT, which *raises* the counter but never reserves a range,
            // so a later seed migration eventually collides with whatever ids users have taken. Splitting the
            // ranges up front is the permanent fix: system exercises live below 100 000, user exercises above,
            // and inserting a lower explicit id never drags the counter back down (SQL Server only raises it).
            //
            // Done here, in the table's very first migration, rather than after the first collision.
            migrationBuilder.Sql(
                "IF IDENT_CURRENT('Exercises') < 100000 " +
                "DBCC CHECKIDENT ('Exercises', RESEED, 100000);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplementIntakes");

            migrationBuilder.DropTable(
                name: "WorkoutRoutineExercises");

            migrationBuilder.DropTable(
                name: "WorkoutSets");

            migrationBuilder.DropTable(
                name: "SupplementScheduleSlots");

            migrationBuilder.DropTable(
                name: "WorkoutSessionExercises");

            migrationBuilder.DropTable(
                name: "Supplements");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");

            migrationBuilder.DropTable(
                name: "WorkoutRoutines");

            migrationBuilder.DropColumn(
                name: "WeightUnit",
                table: "UserProfiles");
        }
    }
}
