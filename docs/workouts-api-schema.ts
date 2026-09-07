/**
 * GoodieHabits — Workouts & Supplements API schema (for the mobile/front-end team).
 *
 * Hand-written from the backend DTOs/commands (Application/Workouts/*, Application/Supplements/*).
 * Generated contract: docs/swagger.json. Reference: docs/workouts-module.md.
 *
 * Serialization conventions (match the backend's System.Text.Json setup):
 *  - Enums serialize as STRINGS (e.g. "RepsAndWeight", "PreWorkout") via JsonStringEnumConverter.
 *  - `DateOnly` serializes as an ISO calendar date "YYYY-MM-DD" — no time, no timezone.
 *  - `TimeOnly` serializes as "HH:mm:ss".
 *  - `DateTime` serializes as an ISO-8601 UTC timestamp ("...Z").
 *  - C# `decimal` -> number; nullable value types -> `T | null`.
 *  - Every endpoint is authenticated; the user comes from the JWT and is never sent in a body.
 *
 * ⚠️ Two calendar-vs-instant rules run through the whole module:
 *  - `performedOn` / `takenOn` / `date` are CALENDAR DATES in the user's own calendar. Format them
 *    from local device time, never from `toISOString()` — see the FE guide for the helper.
 *  - `startedAt` / `completedAt` / `takenAt` are real UTC instants.
 */

// ══════════════════════════════ Enums ══════════════════════════════

/**
 * Declares which measurements a set of this exercise must carry. The client picks its input
 * controls from this field — the backend knows nothing about the UI.
 *
 *   Reps            -> reps
 *   RepsAndWeight   -> reps + weight
 *   Time            -> durationSeconds
 *   Distance        -> distance
 *   DistanceAndTime -> distance + durationSeconds
 *
 * Extra measurements beyond the required ones are ACCEPTED and stored (that is how a weighted
 * pull-up is logged against a Reps exercise). Only the required ones are enforced.
 *
 * ⚠️ Value type: an omitted `metricType` deserializes to "Reps" (0), not null. Always send it.
 */
export type ExerciseMetric =
  | 'Reps'
  | 'RepsAndWeight'
  | 'Time'
  | 'Distance'
  | 'DistanceAndTime';

export type MuscleGroup =
  | 'Other'
  | 'Chest'
  | 'Back'
  | 'Shoulders'
  | 'Biceps'
  | 'Triceps'
  | 'Forearms'
  | 'Abs'
  | 'Glutes'
  | 'Quadriceps'
  | 'Hamstrings'
  | 'Calves'
  | 'FullBody'
  | 'Cardio';

export type Equipment =
  | 'None'          // bodyweight
  | 'Barbell'
  | 'Dumbbell'
  | 'Kettlebell'
  | 'Machine'
  | 'Cable'
  | 'ResistanceBand'
  | 'Other'
  | 'Calisthenics'  // skill work: muscle-ups, levers, pistols
  | 'Rings';        // gymnastic rings

export type WorkoutSessionStatus = 'InProgress' | 'Completed' | 'Abandoned';

/** `DropSet` / `FailureSet` are RESERVED for a future release and are never produced today. */
export type WorkoutSetType = 'Normal' | 'Warmup' | 'DropSet' | 'FailureSet';

export type SupplementUnit =
  | 'Piece'
  | 'Capsule'
  | 'Tablet'
  | 'Gram'
  | 'Milligram'
  | 'Milliliter'
  | 'Scoop'
  | 'Drop'
  | 'InternationalUnit';

/** `Custom` requires `timeOfDay`. `PreWorkout`/`PostWorkout` are what the in-training panel filters on. */
export type SupplementTiming =
  | 'Morning'
  | 'Midday'
  | 'Afternoon'
  | 'Evening'
  | 'Night'
  | 'PreWorkout'
  | 'PostWorkout'
  | 'WithMeal'
  | 'Custom';

// ═══════════════════════════ Shared shapes ═════════════════════════

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// ════════════════════════════ Exercises ════════════════════════════

export interface ExerciseDto {
  id: number;
  name: string;
  metricType: ExerciseMetric;
  muscleGroup: MuscleGroup;
  equipment: Equipment;
  note: string | null;
  /** Seeded and shared by everyone. Cannot be edited, archived or deleted (403). */
  isSystem: boolean;
  /** Hidden from pickers, but still present in routines and past sessions. */
  isArchived: boolean;
}

export interface CreateExerciseRequest {
  name: string;
  metricType: ExerciseMetric;
  muscleGroup?: MuscleGroup;    // default "Other"
  equipment?: Equipment;        // default "None"
  note?: string | null;
}

/** Full replacement — send every field. Changing metricType does not affect past sessions. */
export interface UpdateExerciseRequest {
  name: string;
  metricType: ExerciseMetric;
  muscleGroup?: MuscleGroup;
  equipment?: Equipment;
  note?: string | null;
}

export interface SetArchivedRequest {
  isArchived: boolean;
}

// ════════════════════════════ Routines ═════════════════════════════

export interface WorkoutRoutineExerciseDto {
  id: number;
  exerciseId: number;
  exerciseName: string;
  metricType: ExerciseMetric;
  muscleGroup: MuscleGroup;
  /** 0-based. Mirrors the array order; the server maintains it. */
  order: number;
  targetSets: number | null;
  targetReps: number | null;
  targetWeight: number | null;
  targetDurationSeconds: number | null;
  targetDistance: number | null;
  restSeconds: number | null;
  note: string | null;
}

export interface WorkoutRoutineDto {
  id: number;
  name: string;
  description: string | null;
  isArchived: boolean;
  exercises: WorkoutRoutineExerciseDto[];
}

/**
 * ⚠️ There is no `order` field — THE POSITION IN THE ARRAY IS THE ORDER.
 * Every target is optional; a routine that only fixes the sequence is a valid plan.
 */
export interface RoutineExerciseInput {
  exerciseId: number;
  targetSets?: number | null;
  targetReps?: number | null;
  targetWeight?: number | null;
  targetDurationSeconds?: number | null;
  targetDistance?: number | null;
  restSeconds?: number | null;
  note?: string | null;
}

export interface CreateRoutineRequest {
  name: string;
  description?: string | null;
  /** Omitted or [] is legal — a routine under construction is not an error. */
  exercises?: RoutineExerciseInput[];
}

/** FULL replacement of the routine and its exercise list. Items left out are removed. */
export interface UpdateRoutineRequest {
  name: string;
  description?: string | null;
  exercises?: RoutineExerciseInput[];
}

// ════════════════════════════ Sessions ═════════════════════════════

export interface WorkoutSetDto {
  id: number;
  /** 1-based and contiguous. The server assigns and renumbers it. */
  setNumber: number;
  reps: number | null;
  weight: number | null;
  durationSeconds: number | null;
  distance: number | null;
  rpe: number | null;
  setType: WorkoutSetType;
  /** UTC instant. */
  completedAt: string;
  /** Epley estimate, or null when the set has no usable reps/weight pair (or reps > 30). */
  estimatedOneRepMax: number | null;
}

export interface WorkoutSessionExerciseDto {
  id: number;
  /** null once the library exercise has been deleted — render from the snapshot below. */
  exerciseId: number | null;
  /** ⚠️ SNAPSHOT taken when the exercise was added. Renaming the library row never rewrites history. */
  exerciseName: string;
  /** ⚠️ SNAPSHOT. Render this session's inputs from it, not from the library's current metric. */
  metricType: ExerciseMetric;
  order: number;
  targetSets: number | null;
  targetReps: number | null;
  targetWeight: number | null;
  targetDurationSeconds: number | null;
  targetDistance: number | null;
  restSeconds: number | null;
  note: string | null;
  sets: WorkoutSetDto[];
}

/** Warm-up sets are excluded from every number here. */
export interface WorkoutSessionTotalsDto {
  exerciseCount: number;
  setCount: number;
  totalReps: number;
  /** Sum of reps × weight, in the user's weight unit (GET /api/workouts/settings). */
  totalVolume: number;
}

export interface WorkoutSessionSummaryDto {
  id: number;
  /** The template it came from, or null for an ad-hoc session (or a deleted routine). */
  routineId: number | null;
  name: string;
  /** Calendar date "YYYY-MM-DD". */
  performedOn: string;
  /** UTC instant. */
  startedAt: string;
  completedAt: string | null;
  status: WorkoutSessionStatus;
  note: string | null;
  /** Wall-clock length, or null while still running. */
  durationSeconds: number | null;
  totals: WorkoutSessionTotalsDto;
}

/** Returned by GET /{id}, GET /active and EVERY mutating session endpoint. */
export interface WorkoutSessionDto extends WorkoutSessionSummaryDto {
  exercises: WorkoutSessionExerciseDto[];
}

/**
 * Either `routineId` (materializes that template) or `name` (ad-hoc — then name is REQUIRED).
 * `performedOn` defaults to the user's local today, derived from their profile timezone.
 */
export interface StartSessionRequest {
  routineId?: number | null;
  name?: string | null;
  performedOn?: string | null;
  note?: string | null;
}

/** Metadata only. The exercise log is edited through the log endpoints. */
export interface UpdateSessionRequest {
  name: string;
  performedOn: string;
  note?: string | null;
}

/**
 * Set numbering is the server's job — position in the array is the set number.
 * `completedAt` is optional and defaults to now; send the real instant when replaying an
 * offline session so the timestamps aren't all bunched at sync time.
 */
export interface SessionSetInput {
  reps?: number | null;
  weight?: number | null;
  durationSeconds?: number | null;
  distance?: number | null;
  rpe?: number | null;
  setType?: WorkoutSetType;   // default "Normal"
  completedAt?: string | null;
}

/** Position in the array is the order — no `order` field, same as routines. */
export interface SessionExerciseInput {
  exerciseId: number;
  targetSets?: number | null;
  targetReps?: number | null;
  targetWeight?: number | null;
  targetDurationSeconds?: number | null;
  targetDistance?: number | null;
  restSeconds?: number | null;
  note?: string | null;
  sets?: SessionSetInput[];
}

/**
 * ⚠️ FULL REPLACEMENT of the session's whole exercise+set tree — this is what makes it safe to
 * retry after a flaky gym connection. Sending the same payload twice leaves the same log, not a
 * doubled one. An empty array CLEARS the log, so always send the whole tree.
 */
export interface LogSessionRequest {
  exercises: SessionExerciseInput[];
}

// ════════════════════════ Workout analytics ════════════════════════

export interface MuscleGroupVolumeDto {
  muscleGroup: MuscleGroup;
  setCount: number;
  totalReps: number;
  totalVolume: number;
}

/** Counts COMPLETED sessions only, and excludes warm-up sets. */
export interface WorkoutSummaryDto {
  from: string;
  to: string;
  sessionCount: number;
  exerciseCount: number;
  setCount: number;
  totalReps: number;
  totalVolume: number;
  totalDurationSeconds: number;
  averageDurationSeconds: number | null;
  byMuscleGroup: MuscleGroupVolumeDto[];
}

export interface ExerciseHistoryPointDto {
  sessionId: number;
  performedOn: string;
  setCount: number;
  totalReps: number;
  totalVolume: number;
  maxWeight: number | null;
  bestEstimatedOneRepMax: number | null;
}

export interface ExerciseHistoryDto {
  exerciseId: number;
  exerciseName: string;
  from: string;
  to: string;
  /** Oldest first. Sessions where the exercise was only warmed up on are omitted. */
  points: ExerciseHistoryPointDto[];
}

/**
 * All-time bests. Note there is deliberately NO estimated 1RM here — use exercise-history for
 * that number. Exercises deleted from the library drop out of records entirely.
 */
export interface PersonalRecordDto {
  exerciseId: number;
  exerciseName: string;
  maxWeight: number | null;
  maxReps: number | null;
  maxSetVolume: number | null;
  setCount: number;
  lastPerformedOn: string;
}

// ════════════════════════ Workout settings ═════════════════════════

/**
 * ⚠️ Changing the weight unit REINTERPRETS stored weights; it never converts them. Same rule as
 * the finance currency setting.
 */
export interface WorkoutSettingsDto {
  weightUnit: string;             // "kg" | "lb"
  supportedWeightUnits: string[];
}

export interface UpdateWeightUnitRequest {
  weightUnit: string;
}

// ═══════════════════════════ Supplements ═══════════════════════════

export interface SupplementSlotDto {
  id: number;
  timing: SupplementTiming;
  /** "HH:mm:ss" local clock time. Required when timing is "Custom". */
  timeOfDay: string | null;
  /** Signed minutes relative to the workout: -30 = "30 min przed treningiem". */
  offsetMinutes: number | null;
  /** In the parent supplement's unit. */
  amount: number;
  note: string | null;
}

export interface SupplementDto {
  id: number;
  name: string;
  /** The unit for this supplement AND all of its slots and intakes. */
  unit: SupplementUnit;
  defaultAmount: number | null;
  note: string | null;
  color: string | null;           // hex "#RRGGBB"
  icon: string | null;            // Ionicons outline name
  /** Inactive drops it off the checklist but keeps every dose ever logged. */
  isActive: boolean;
  slots: SupplementSlotDto[];
}

export interface CreateSupplementRequest {
  name: string;
  unit: SupplementUnit;
  defaultAmount?: number | null;
  note?: string | null;
  color?: string | null;
  icon?: string | null;
}

export interface UpdateSupplementRequest {
  name: string;
  unit: SupplementUnit;
  defaultAmount?: number | null;
  note?: string | null;
  color?: string | null;
  icon?: string | null;
}

export interface SetActiveRequest {
  isActive: boolean;
}

/** Shared by POST /slots and PUT /slots/{slotId}. All slot endpoints return the SupplementDto. */
export interface SupplementSlotRequest {
  timing: SupplementTiming;
  amount: number;
  timeOfDay?: string | null;
  offsetMinutes?: number | null;
  note?: string | null;
}

// ══════════════════════ Checklist & intakes ════════════════════════

export interface SupplementIntakeDto {
  id: number;
  supplementId: number;
  supplementName: string;
  unit: SupplementUnit;
  /** null for an ad-hoc dose, or one whose slot has since been deleted. */
  scheduleSlotId: number | null;
  /** Calendar date "YYYY-MM-DD". */
  takenOn: string;
  /** UTC instant. */
  takenAt: string;
  amount: number;
  /** Set only when ticked from the training screen. */
  workoutSessionId: number | null;
}

export interface SupplementChecklistItemDto {
  supplementId: number;
  supplementName: string;
  unit: SupplementUnit;
  color: string | null;
  icon: string | null;
  slotId: number;
  timing: SupplementTiming;
  timeOfDay: string | null;
  offsetMinutes: number | null;
  /** What the schedule says. */
  plannedAmount: number;
  note: string | null;
  /** The checkbox. */
  taken: boolean;
  intakeId: number | null;
  takenAt: string | null;
  /** What was actually logged — can differ from plannedAmount. */
  takenAmount: number | null;
  workoutSessionId: number | null;
}

/**
 * Returned by GET /checklist AND by every intake mutation, so the client repaints from one
 * response. `items` covers active supplements only; `adHoc` can include inactive ones.
 */
export interface SupplementChecklistDto {
  date: string;
  items: SupplementChecklistItemDto[];
  adHoc: SupplementIntakeDto[];
}

/**
 * The checkbox. Send the INTENT, not a row id. Idempotent both ways: ticking twice leaves one
 * dose, un-ticking something never ticked is a silent no-op.
 * `amount` falls back to the slot's planned amount.
 */
export interface ToggleIntakeRequest {
  supplementId: number;
  slotId: number;
  date: string;
  taken: boolean;
  amount?: number | null;
  workoutSessionId?: number | null;
}

/**
 * A dose no slot planned. Deliberately REPEATABLE, unlike the checkbox — posting twice records
 * two doses. Undo with DELETE /api/supplements/intakes/{id}.
 * `amount` falls back to the supplement's defaultAmount; a 400 follows if there is neither.
 */
export interface LogAdHocIntakeRequest {
  supplementId: number;
  date: string;
  amount?: number | null;
  workoutSessionId?: number | null;
}

// ═════════════════════ Supplement analytics ════════════════════════

export interface SupplementAdherenceItemDto {
  supplementId: number;
  supplementName: string;
  slotsPerDay: number;
  scheduled: number;
  taken: number;
  /** 0-100, NOT clamped. `null` means "nothing evaluated yet" — do not render it as 0%. */
  rate: number | null;
}

/**
 * ⚠️ A day counts toward `scheduled` only once it has FULLY ELAPSED in the user's local calendar,
 * or once something was taken that day. Today never drags the number down while it is running.
 * Ad-hoc doses are excluded from `taken` — they were never scheduled.
 */
export interface SupplementAdherenceReportDto {
  from: string;
  to: string;
  scheduled: number;
  taken: number;
  rate: number | null;
  items: SupplementAdherenceItemDto[];
}

// ═══════════════════════════ ROUTES ════════════════════════════════

/**
 * All routes are authenticated (Bearer JWT).
 *
 * Exercises
 *   GET    /api/workouts/exercises                ?muscleGroup= &metricType= &equipment= &search= &includeArchived=
 *                                                 -> ExerciseDto[]   (system rows + your own)
 *   POST   /api/workouts/exercises                CreateExerciseRequest      -> ExerciseDto
 *   PUT    /api/workouts/exercises/{id}           UpdateExerciseRequest      -> ExerciseDto
 *   PATCH  /api/workouts/exercises/{id}/archived  SetArchivedRequest         -> ExerciseDto
 *   DELETE /api/workouts/exercises/{id}                                      -> 204
 *          409 while a routine still plans it (message names the routines).
 *          403 on a system exercise. Past sessions never block deletion.
 *
 * Routines
 *   GET    /api/workouts/routines                 ?includeArchived=          -> WorkoutRoutineDto[]
 *   GET    /api/workouts/routines/{id}                                       -> WorkoutRoutineDto
 *   POST   /api/workouts/routines                 CreateRoutineRequest       -> WorkoutRoutineDto
 *   PUT    /api/workouts/routines/{id}            UpdateRoutineRequest       -> WorkoutRoutineDto
 *   PATCH  /api/workouts/routines/{id}/archived   SetArchivedRequest         -> WorkoutRoutineDto
 *   DELETE /api/workouts/routines/{id}                                       -> 204
 *          Sessions performed from it are KEPT; they just lose routineId.
 *
 * Sessions  (every mutating endpoint returns the full WorkoutSessionDto)
 *   GET    /api/workouts/sessions                 ?from= &to= &status= &page= &pageSize=
 *                                                 -> PagedResult<WorkoutSessionSummaryDto>
 *   GET    /api/workouts/sessions/active          -> 200 WorkoutSessionDto, or 204 NO CONTENT
 *          ⚠️ 204 has an EMPTY BODY — do not call response.json() on it. Never 404: "nothing in
 *          progress" is an ordinary state, not an error.
 *   GET    /api/workouts/sessions/{id}            -> WorkoutSessionDto
 *   POST   /api/workouts/sessions                 StartSessionRequest
 *          409 if one is already in progress; the message carries its id.
 *   PUT    /api/workouts/sessions/{id}            UpdateSessionRequest        (metadata only)
 *   PUT    /api/workouts/sessions/{id}/log        LogSessionRequest           (bulk, idempotent)
 *   POST   /api/workouts/sessions/{id}/finish
 *   POST   /api/workouts/sessions/{id}/abandon
 *   POST   /api/workouts/sessions/{id}/exercises  SessionExerciseInput
 *   DELETE /api/workouts/sessions/{id}/exercises/{entryId}
 *   POST   /api/workouts/sessions/{id}/exercises/{entryId}/sets          SessionSetInput
 *   PUT    /api/workouts/sessions/{id}/exercises/{entryId}/sets/{setId}  SessionSetInput
 *   DELETE /api/workouts/sessions/{id}/exercises/{entryId}/sets/{setId}
 *   DELETE /api/workouts/sessions/{id}                                        -> 204
 *
 * Workout analytics / settings
 *   GET    /api/workouts/analytics/summary            ?from= &to=      -> WorkoutSummaryDto
 *   GET    /api/workouts/analytics/exercise-history   ?exerciseId= &from= &to= -> ExerciseHistoryDto
 *   GET    /api/workouts/analytics/personal-records                    -> PersonalRecordDto[]
 *   GET    /api/workouts/settings                                      -> WorkoutSettingsDto
 *   PUT    /api/workouts/settings/weight-unit     UpdateWeightUnitRequest -> WorkoutSettingsDto
 *
 * Supplements  (every slot endpoint returns SupplementDto)
 *   GET    /api/supplements                       ?includeInactive=    -> SupplementDto[]
 *   POST   /api/supplements                       CreateSupplementRequest
 *   PUT    /api/supplements/{id}                  UpdateSupplementRequest
 *   PATCH  /api/supplements/{id}/active           SetActiveRequest
 *   DELETE /api/supplements/{id}                                       -> 204
 *          409 once anything is logged against it — deactivate instead.
 *   POST   /api/supplements/{id}/slots            SupplementSlotRequest
 *   PUT    /api/supplements/{id}/slots/{slotId}   SupplementSlotRequest
 *   DELETE /api/supplements/{id}/slots/{slotId}
 *          Doses already taken are kept and become ad-hoc records.
 *
 * Checklist & intakes  (every mutation returns SupplementChecklistDto for that date)
 *   GET    /api/supplements/checklist             ?date= &timing=…&timing=…
 *   GET    /api/supplements/intakes               ?from= &to=          -> SupplementIntakeDto[]
 *   PUT    /api/supplements/intakes               ToggleIntakeRequest   (idempotent checkbox)
 *   POST   /api/supplements/intakes               LogAdHocIntakeRequest (repeatable)
 *   DELETE /api/supplements/intakes/{intakeId}
 *
 * Supplement analytics
 *   GET    /api/supplements/analytics/adherence   ?from= &to= -> SupplementAdherenceReportDto
 */
