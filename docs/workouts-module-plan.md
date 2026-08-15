# Workouts & Supplements — build plan

> 🗑️ **SUPERSEDED (2026-08-15) by [`docs/workouts-module.md`](./workouts-module.md) + `ARCHITECTURE.md` §14.**
> Everything still true has been folded into those. This file is only the build log now — **delete it once
> it has been committed at least once**, the way `FINANCE_MODULE_PLAN.md` was. It is deliberately still here
> because it has never been committed, so deleting it today would lose it for good rather than parking it in
> git history.

> **Status: core complete (2026-08-15). Phases 1-5 are built and green — 668 tests. Only phase 6 (docs and
> the FE handoff) is left.**
> This is the phase-by-phase build checklist plus the decisions taken up front and *why*. When the module
> ships, fold the durable parts into `docs/workouts-module.md` (the reference) + `ARCHITECTURE.md` §14 and
> delete this file — exactly what was done with `FINANCE_MODULE_PLAN.md`.

Two new feature domains, both keyed on **`UserProfileId`** (not the auth `Account`), following every convention
in [`ARCHITECTURE.md`](../ARCHITECTURE.md). The [finance module](./finance-module.md) is the reference
implementation of this recipe end to end; where a decision below mirrors one of its, the reason is cited rather
than re-argued.

---

## 1. Scope

| Domain | What it owns |
|---|---|
| **`Workouts`** | Exercise library, workout templates ("routines"), performed sessions with logged sets, training analytics |
| **`Supplements`** | Supplement catalog, intake schedule, daily intake log ("checklist"), adherence analytics |

**They are siblings, not parent/child.** The only link is a nullable `WorkoutSessionId` on an intake row.
This is forced by the requirement *"przydałoby się też poza sekcją treningową"*: the supplement checklist must
work on a rest day, so it cannot hang off a session. The in-training supplement panel is then just a filtered
view of the same resource — no second entity, no duplicated logic. Same posture as *"Finance stays independent
of Quests"*.

### Owner decisions locked at kickoff (2026-08-15)

| Question | Decision |
|---|---|
| "Rutyny" vs "zestawy ćwiczeń" | **One level.** `WorkoutRoutine` = one runnable workout template. Multi-week plans are a later layer *above* it, requiring no change to these entities. |
| Session logging protocol | **Both.** Granular per-set `POST` for live logging **and** a bulk full-replacement `PUT .../log` for offline sync / finish. |
| Gamification | **Hook now, rewards later.** `WorkoutSessionCompletedEvent` + a `BadgeTriggerEnum` slot exist from day one; no strategy awards XP yet. |
| Exercise library | **Seeded system rows + unlimited user rows** (`FinanceCategory` pattern). Empty-library cold start is the main adoption killer for this kind of module. |

---

## 2. Decisions (locked before writing code)

### Time & units

- **`WorkoutSession.PerformedOn` and `SupplementIntake.TakenOn` are `DateOnly` (SQL `date`).**
  Not negotiable — this is the third time the repo meets this trap. `UserProfile.TimeZone` is rewritten on
  *every token refresh*, so "which training day was this" must be a calendar fact, never a derived UTC instant.
  See `FinanceTransaction.OccurredOn` and ARCHITECTURE §6.
- **`StartedAt` / `CompletedAt` / `TakenAt` stay UTC `DateTime`** — "when did I start lifting" is a genuine
  instant, and it is what any time-of-day analytics would read. Same split as `OccurredOn` vs `CreatedAt`.
- **Weight is `decimal(6,2)`, distance `decimal(8,2)`, stored exactly as the user typed it.**
- **Unit system is one setting per user: `UserProfile.WeightUnit`** (`"kg"` \| `"lb"`, default `"kg"`,
  allow-list in `Domain/ValueObjects/SupportedWeightUnits.cs`). **Changing it does not convert history** —
  identical call to `UserProfile.Currency`, for the identical reason. A per-set unit column was rejected: it
  doubles the width of the module's largest table to serve a case that does not occur (nobody logs half a
  session in pounds).
- Use the injected **`IClock`**, never `SystemClock.Instance` (ARCHITECTURE §9).

### Exercises

- **A single `Exercise` entity discriminated by `MetricType`** — no class hierarchy. Exactly the `Quest` /
  `FinanceTransaction` shape. The metric declares which columns on a logged set are meaningful; all metric
  columns on the set are nullable.

  | `ExerciseMetricEnum` | Required on a set | Example |
  |---|---|---|
  | `Reps` | `Reps` | podciąganie, pompki |
  | `RepsAndWeight` | `Reps`, `Weight` | wyciskanie, przysiad |
  | `Time` | `DurationSeconds` | plank, zwis |
  | `Distance` | `Distance` | spacer farmera |
  | `DistanceAndTime` | `Distance`, `DurationSeconds` | bieg, rower |

  This buys cardio and isometrics for free, and it answers *"buttony albo input do podawania wartości"*
  without the backend knowing anything about the UI: the client picks its control from `metricType`.
- **System rows carry `UserProfileId = null`**, user rows carry theirs (`FinanceCategory` / `Badge` pattern).
  A user may not edit or delete a system row; they create their own instead.
- **Unique `(UserProfileId, Name)` on user rows** (filtered index) → `Conflict` on a duplicate.
- **Delete is blocked while the exercise is referenced by a routine** (409 naming the offending routines) —
  the finance category rule. It is **not** blocked by session history, because history snapshots the name
  (below). `IsArchived` is the retire-without-deleting path: archived exercises disappear from the picker and
  stay in routines and history.

### Routines → sessions

- **A routine is a template and a session is a record; the session never writes back.** This is the
  `RecurringTransaction → FinanceTransaction` relationship and it inherits all of its rules:
  editing a routine does **not** touch past sessions; deleting a routine **nulls `RoutineId`** on its sessions
  in the same unit of work (FK stays `Restrict` in the DB) rather than deleting them.
- **`WorkoutSessionExercise` snapshots `ExerciseName` *and* `MetricType`** alongside a nullable `ExerciseId`.
  Without the snapshot, renaming "Wyciskanie" to "Wyciskanie sztangi" silently rewrites a year of history, and
  changing an exercise's metric makes old rows unrenderable. The FK is `SetNull`.
- **`PUT /routines/{id}` is full replacement of the exercise list** (matching `PUT /transactions`), keyed on
  `order`. Partial patching of an ordered collection is a bug farm.
- **At most one `InProgress` session per user.** Starting a second returns `409 Conflict` **carrying the
  active session** so the client can offer "wznów". This is a deliberate step back from the module's general
  flexibility posture: two concurrent sessions is not a feature, it is a state the UI cannot draw.
- **A session may be incomplete and that is legal.** No validation demands that planned sets were performed; a
  skipped exercise simply has no sets. *Flexibility, not restriction.*
- **Both logging paths write the same rows and must stay interchangeable.** `POST .../sets` appends one set;
  `PUT /sessions/{id}/log` replaces the session's whole exercise+set tree in one `SaveChangesAsync`. The bulk
  path is what makes a no-signal gym session survivable, so it is **idempotent by construction** (full
  replacement, not append).

### Supplements

- **`Supplement` (what) and `SupplementScheduleSlot` (when + how much) are separate.** Magnesium morning *and*
  evening is one supplement with two slots, not two supplements. This is a direct read of the requirement:
  a table of names, a second section describing timing and amount, and a checkbox.
- **`Unit` lives on the supplement, `Amount` on the slot.** The slot inherits the unit — same inheritance call
  as a finance sub-category inheriting `Type`/`IsSavings`, and for the same reason: a supplement whose slots
  disagree about the unit makes every aggregate meaningless.
- **`UNIQUE (ScheduleSlotId, TakenOn)`** (filtered to non-null slots) makes double-checking the same slot on
  the same day structurally impossible. This is the `UNIQUE (QuestId, PeriodStart)` lesson, applied *before*
  it costs a three-step migration this time.
- **The checkbox is an idempotent `PUT`, not `POST` + `DELETE`.** A phone taps fast, offline, and twice. The
  client sends the *intent* (`taken: true|false` for a slot on a date), not a row id it would have to track.
- **`ScheduleSlotId = null` is a legal ad-hoc intake** — "wziąłem dziś jeszcze jedną". A model that can only
  record planned doses punishes the user for the exact deviation worth recording.
- **Deleting a supplement is blocked while it has intakes** (`IsActive = false` is the retire path);
  deleting a *slot* nulls `ScheduleSlotId` on its intakes, turning them into historical ad-hoc rows —
  the `RecurringTransactionId` nulling pattern.
- **v1 slots are daily.** Weekday selection (`SupplementSlotDay`, mirroring `WeeklyQuest_Day`) is deferred; it
  is purely additive. `Timing = PreWorkout` already covers the "training days only" case, because the
  in-training panel filters by timing.

### Analytics

- **Aggregate in memory over a range query** (`GetForPeriodAsync`), like all five finance analytics handlers.
- **Personal records are a grouped SQL projection**, not a denormalized column — a projection type in the
  `MonthlyTotal` mould. Same reasoning as `openingBalance`: a fold over all history is a few dozen grouped
  rows at this scale, and denormalizing would buy only drift risk.
- **Supplement adherence reuses the quest denominator rule:** a scheduled slot-day counts against you only
  once that day has fully elapsed relative to the user's local today, and adherence is `null` when nothing has
  been evaluated. Today at 09:00 must not read as "33% adherence".
- **Estimated 1RM is Epley** (`weight × (1 + reps/30)`), in a pure `OneRepMaxCalculator`, computed server-side
  so every client agrees on the number.

---

## 3. Data model

### Workouts

**`Exercise`** — `Id`, `UserProfileId?` (null = system), `Name` (100), `MetricType`, `MuscleGroup`,
`Equipment`, `Note?` (500), `IsArchived`.
Unique `(UserProfileId, Name)` filtered to user rows.

**`WorkoutRoutine`** — `Id`, `UserProfileId`, `Name` (100), `Description?` (500), `IsArchived`.
Owns `ICollection<WorkoutRoutineExercise>`.

**`WorkoutRoutineExercise`** — `Id`, `WorkoutRoutineId`, `ExerciseId`, `Order` (0-based),
`TargetSets?`, `TargetReps?`, `TargetWeight?`, `TargetDurationSeconds?`, `TargetDistance?`,
`RestSeconds?`, `Note?` (250). Unique `(WorkoutRoutineId, Order)`.

**`WorkoutSession`** *(aggregate root)* — `Id`, `UserProfileId`, `RoutineId?`, `Name` (100, snapshot),
`PerformedOn` (`DateOnly`), `StartedAt` (UTC), `CompletedAt?` (UTC), `Status`
(`InProgress` \| `Completed` \| `Abandoned`), `Note?` (500).
Filtered unique index enforcing at most one `InProgress` row per `UserProfileId`.

**`WorkoutSessionExercise`** — `Id`, `WorkoutSessionId`, `ExerciseId?`, `ExerciseName` (100, snapshot),
`MetricType` (snapshot), `Order`, `TargetSets?`, `TargetReps?`, `TargetWeight?`, `RestSeconds?`, `Note?`.

**`WorkoutSet`** — `Id`, `WorkoutSessionExerciseId`, `SetNumber` (1-based), `Reps?`, `Weight?`,
`DurationSeconds?`, `Distance?`, `Rpe?` (`decimal(3,1)`, 1–10), `SetType`
(`Normal` \| `Warmup` \| **`DropSet`** \| **`FailureSet`** — the last two are *reserved slots*, unused in v1,
like `FinanceTransactionTypeEnum.Transfer`), `CompletedAt` (UTC).
Unique `(WorkoutSessionExerciseId, SetNumber)`.

### Supplements

**`Supplement`** — `Id`, `UserProfileId`, `Name` (100), `Unit` (`SupplementUnitEnum`), `DefaultAmount?`,
`Note?` (500), `Color?` (7), `Icon?` (50, Ionicons outline name — the finance category convention),
`IsActive`. Unique `(UserProfileId, Name)`.

**`SupplementScheduleSlot`** — `Id`, `SupplementId`, `Timing` (`Morning` \| `Midday` \| `Afternoon` \|
`Evening` \| `Night` \| `PreWorkout` \| `PostWorkout` \| `WithMeal` \| `Custom`), `TimeOfDay?` (`TimeOnly`),
`OffsetMinutes?` (signed, relative to the workout — `-30` is *"30 min przed treningiem"*), `Amount`,
`Note?` (250).

**`SupplementIntake`** — `Id`, `UserProfileId` (denormalized, so range queries and ownership checks need no
join), `SupplementId`, `ScheduleSlotId?`, `TakenOn` (`DateOnly`), `TakenAt` (UTC), `Amount`,
`WorkoutSessionId?`. Unique `(ScheduleSlotId, TakenOn)` filtered to non-null slots.

### Enums to add

`ExerciseMetricEnum`, `MuscleGroupEnum`, `EquipmentEnum`, `WorkoutSessionStatusEnum`, `WorkoutSetTypeEnum`,
`SupplementUnitEnum`, `SupplementTimingEnum`; plus `WorkoutSessionCompleted` on `BadgeTriggerEnum`.

### Calculators (pure, DB-free, `Domain/Calculators/`)

`WorkoutVolumeCalculator` (Σ reps × weight, per session / per muscle group), `OneRepMaxCalculator` (Epley),
`SupplementAdherenceCalculator` (scheduled vs taken slot-days, with the elapsed-period denominator rule).

### Repositories on `IUnitOfWork`

`IExerciseRepository`, `IWorkoutRoutineRepository`, `IWorkoutSessionRepository`, `ISupplementRepository`,
`ISupplementIntakeRepository`.

---

## 4. API surface (draft)

All `[Authorize]`, identity via `User.GetCurrentUserProfileId()`. Enums serialize as strings; `DateOnly` is
`"YYYY-MM-DD"`; `TimeOnly` is `"HH:mm:ss"`.

| Route | Verbs |
|---|---|
| `api/workouts/exercises` ✅ | `GET` (system + own; filters `muscleGroup`/`metricType`/`search`/`includeArchived`) · `POST` · `PUT /{id}` · `PATCH /{id}/archived` · `DELETE /{id}` |
| `api/workouts/routines` ✅ | `GET` (`?includeArchived`) · `GET /{id}` · `POST` · `PUT /{id}` (full replacement) · `PATCH /{id}/archived` · `DELETE /{id}` |
| `api/workouts/sessions` ✅ | `GET` (`from`/`to`/`status` + paging → `PagedResult<WorkoutSessionSummaryDto>`) · `GET /{id}` · `GET /active` (200 + `null` when none) · `POST` (start) · `PUT /{id}` (metadata only) · `DELETE /{id}` |
| `api/workouts/sessions/{id}/log` ✅ | `PUT` (bulk full replacement of the exercise+set tree; idempotent) |
| `api/workouts/sessions/{id}/exercises` ✅ | `POST` (add mid-session, optionally with its sets) · `DELETE /{entryId}` |
| `api/workouts/sessions/{id}/exercises/{entryId}/sets` ✅ | `POST` · `PUT /{setId}` · `DELETE /{setId}` |
| `api/workouts/sessions/{id}/finish` ✅ | `POST` (→ `Completed`, raises the domain event) |
| `api/workouts/sessions/{id}/abandon` ✅ | `POST` (→ `Abandoned`, deliberately raises nothing) |
| `api/workouts/analytics` ✅ | `GET summary` (`from`/`to`) · `exercise-history` (`exerciseId`/`from`/`to`) · `personal-records` |
| `api/workouts/settings` ✅ | `GET` · `PUT weight-unit` |
| `api/supplements` ✅ | `GET` (`?includeInactive`) · `POST` · `PUT /{id}` · `PATCH /{id}/active` · `DELETE /{id}` |
| `api/supplements/{id}/slots` ✅ | `POST` · `PUT /{slotId}` · `DELETE /{slotId}` (all return the supplement) |
| `api/supplements/checklist` ✅ | `GET` (`?date=`, repeatable `?timing=`) |
| `api/supplements/intakes` ✅ | `PUT` (idempotent toggle) · `POST` (ad-hoc dose) · `GET` (`from`/`to`) · `DELETE /{id}` |
| `api/supplements/analytics` ✅ | `GET adherence` (`from`/`to`) |

**`GET /supplements/checklist?date=…`** is the endpoint that serves *both* UI surfaces. It returns every active
supplement's slots for that date with `taken`/`takenAt`, plus that date's ad-hoc intakes. The in-training panel
calls it with `?timing=PreWorkout,PostWorkout` and passes `workoutSessionId` when toggling — that is the whole
integration between the two modules.

---

## 5. Phases

Each phase ends green (`dotnet test Application.Tests`) and is independently shippable.

**Phase 0 — contract.** This document, plus a first `docs/workouts-api-schema.ts` skeleton so the FE agent can
start against the shape before the endpoints exist.

**Phase 1 — Domain + persistence. ✅ Done (2026-08-15).** All entities, enums, calculators, repository
interfaces, EF configurations, `AppDbContext` `DbSet`s, `UnitOfWork` wiring, `UserProfile.WeightUnit`, the
migration, and 89 pure unit tests (suite: 460 → **549**, all green).

*What shipped differently from the sketch above, and why:*

- **One migration, not two** — `20260815130138_AddWorkoutsAndSupplementsModules`. `SupplementIntake` has an FK
  to `WorkoutSession`, so the two modules are a single model diff; splitting them would have meant hand-editing
  generated code for no benefit. It **reseeds the `Exercises` IDENTITY to 100 000** in `Up()`, as required.
  ⚠️ **Not yet applied to the database** — `dotnet ef database update … -- production` still has to be run.
- **`WorkoutSessionExercise` also carries `TargetDurationSeconds` / `TargetDistance`.** Without them,
  materializing a cardio routine into a session silently dropped its targets.
- **`WorkoutSessionExercise.ExerciseId` is `Restrict` + the handler clears it**, not `SetNull`. Same end
  behaviour, but it matches the codebase's established shape for "this row outlives what it points at"
  (`RecurringTransaction`), and avoids any multiple-cascade-path argument with SQL Server.
  `SupplementIntake.WorkoutSessionId` *is* `SetNull` — deleting a session must neither be blocked by nor
  destroy the doses taken during it.
- **No database unique index on any name.** Duplicate-name checks live in handlers (`ExistsByNameAsync` on
  each repository) → friendly `Conflict`. A DB constraint would also have blocked reusing the name of an
  *archived* exercise, which is a legitimate thing to do. The only unique indexes are the two load-bearing
  ones: `(ScheduleSlotId, TakenOn)` filtered, and one `InProgress` session per user filtered on `Status = 0`.
- **New value objects:** `WorkoutLimits` (measurement bounds shared by entity guards and the future validators),
  `WorkoutSessionTotals`, `SupplementAdherence`, `SupportedWeightUnits`.
- **The personal-records SQL projection is deferred to phase 5**, where the query that needs it gets written.
  Phase 1's repositories cover CRUD, ownership, range reads and the detach queries the delete paths need.
- **`Supplement.AddSlot` stamps the slot's `SupplementId`** so `SupplementIntake.Create` can check parentage
  before EF's relationship fixup runs. A guard that only works after a database round-trip is not a guard.

**Phase 2 — Exercises + Routines. ✅ Done (2026-08-15).** Both slices, both controllers, the delete-blocking
rules, `IsArchived`, and the seeded library. 43 tests (suite: 549 → **592**, all green).

- **The system library is 101 Polish-named exercises**, calisthenics-first with enough barbell/dumbbell/machine
  work to log an ordinary gym session. No i18n on the backend — the same call the finance taxonomy made.
  It is a starting point, not a canon: users add their own freely and archive what they don't do.
  **Source of truth is `ExerciseConfiguration.SeedSystemExercises`** (no companion `.txt` — one place, so it
  cannot drift). Migration `20260815131826_SeedSystemExercises`.
  Id blocks, which **must stay stable**: chest 1-99, back 100-199, shoulders 200-299, arms 300-399,
  core 400-499, legs 500-599, full body 600-699, cardio 700-799. Never seed at or above 100 000.
- **Ownership rules, uniform across both slices:** a system row resolves but rejects writes with **403**
  (not 404 — pretending it doesn't exist would send the client hunting for a bug that isn't there); another
  user's row is **404**. Name uniqueness is per user and per slice, so shadowing a system exercise's name is
  legal.
- **Delete blocks name the offenders.** Deleting an exercise is blocked (409) while a *routine* plans it, and
  the message lists the routines. It is never blocked by session history — the handler clears
  `WorkoutSessionExercise.ExerciseId` and the snapshot carries the row.
- **`PATCH /{id}/archived`** on both resources is the retire path when delete is blocked.
  ⚠️ System exercises cannot be archived: hiding a shared row needs per-user state. Deliberate backlog item.
- **The routine exercise array carries no `order` field — position *is* order.** A separate order value the
  server has to reconcile against array order is a contradiction waiting to happen, and both create and update
  replace the list wholesale.
- **Writes re-read before responding** (`RoutineResponseBuilder`). Exercise name and metric live on a
  navigation the freshly written graph hasn't loaded, and attaching the no-tracking rows the handler validated
  against would make EF try to insert them. One extra round-trip buys "the write's response is byte-identical
  to the next `GET`".
- Regression test in place for the decision the module leans on: **editing or deleting a routine never reaches
  a session already performed from it**.

**Phase 3 — Sessions. ✅ Done (2026-08-15).** Start-from-routine materialization, `GET /active`, per-set
logging, bulk `PUT .../log`, finish/abandon, the one-active-session conflict, and the gamification hook.
31 tests (suite: 592 → **623**, all green). No schema change, so no migration.

- **Both logging paths write the same rows and are interchangeable** — that is the contract, not an accident.
  Bulk `PUT .../log` is **full replacement**, so retrying the identical payload after a flaky gym connection
  leaves the same log rather than a doubled one (there is a test for exactly that). An empty array clears the
  log, which is legal — so always send the whole tree.
- **Every mutating endpoint returns the full session**, totals included, so the client repaints from one
  response instead of stitching partial updates. All writes re-read before responding
  (`SessionWriteContext.ReadBackAsync`), same contract as the routines slice.
- **Children are resolved inside the loaded session**, which makes ownership checks free: a set id from
  someone else's session simply isn't in the graph and reads as 404 without a second query.
- **`GET /active` answers 200 with a `null` body** when nothing is in progress. "No active session" is an
  ordinary state; a 404 would make the client treat it as an error.
- **Starting a second session is 409 and the message carries the active session's id**, so the client can
  offer "resume" without another round-trip. The filtered unique index is the backstop underneath.
- **`performedOn` defaults to the user's local today via `UserProfile.LocalDateOn`** — the codebase's single
  source of truth for "what day is it for this user" (ARCHITECTURE §6). Clients may send it explicitly to
  back-fill a session logged on paper.
- **Set-level metric validation lives in the entity, not the validator.** Whether a set carries what its
  exercise's metric requires needs the entry's snapshotted metric, which only the handler has; restating the
  metric table in FluentValidation would create a second version of it. The entity throws, the middleware
  turns it into a 400.
- **Finish raises `WorkoutSessionCompletedEvent`, abandon deliberately does not** — an abandoned session is
  not an achievement and must never feed gamification. Events are published before `SaveChangesAsync` and then
  cleared, matching `DeleteQuestCommandHandler` (there is no central dispatch — ARCHITECTURE §9).
- `SessionSetInput.completedAt` is optional and defaults to now; send the real instant when replaying an
  offline session so timestamps aren't all bunched at sync time.
- No `Program.cs` registration was needed: handlers, validators and Mapster profiles are picked up by the
  existing assembly scans, and `IClock` / `IPublisher` were already registered.

**Phase 4 — Supplements. ✅ Done (2026-08-15).** Catalog, slots, checklist, idempotent toggle, ad-hoc intakes,
session linking. 27 tests (suite: 623 → **650**). No schema change.

- **The checkbox is `PUT /supplements/intakes` and carries intent, not a row id** — `{supplementId, slotId,
  date, taken}`. Idempotent both ways: ticking twice leaves one dose, un-ticking something never ticked is a
  silent no-op. The filtered unique index on `(slot, date)` is the backstop when two taps race.
- **Ad-hoc doses are a separate `POST`, deliberately *not* idempotent.** Repeating an unplanned dose is a real
  event; collapsing two of them would lose a fact the user chose to record. Undo is
  `DELETE /supplements/intakes/{id}`, which is also the only way to remove an ad-hoc dose (it has no slot to
  un-tick).
- **Every intake mutation returns the whole day's checklist**, so the client repaints from one response —
  the same contract as the sessions slice.
- **`GET /checklist?date=&timing=…` serves both UI surfaces.** The standalone screen renders it whole; the
  in-training panel asks for the same date filtered to `PreWorkout`/`PostWorkout` and passes
  `workoutSessionId` when ticking. **That filter is the entire integration between the two modules** — there
  is no session-scoped supplement resource, because the plan has to work on a rest day.
- **Inactive supplements drop out of `items` but still appear in `adHoc`** — the plan is gone, what was
  swallowed is not. Logging an ad-hoc dose of an inactive supplement is allowed for the same reason:
  deactivating means "off my plan", not "I can never take this".
- **Deleting a supplement is refused (409) once anything is logged against it**, pointing at deactivation
  instead. Deleting a *slot* succeeds and **nulls its intakes' `ScheduleSlotId`**, turning them into ad-hoc
  records — `Restrict` alone would block every slot the user had ever ticked, i.e. every slot worth deleting.
- Duplicate timings on one supplement are allowed (two morning doses is a real pattern).

**Phase 5 — Analytics. ✅ Done (2026-08-15).** Summary, exercise history + est. 1RM, personal records,
adherence, plus the weight-unit settings slice the plan called for. 18 tests (suite: 650 → **668**).
No schema change.

- **Only completed sessions count**, everywhere. An abandoned session is not training you did, and an
  in-progress one would make today's numbers move under the user as they log. Warm-ups are excluded from
  every total, in analytics and personal records alike.
- **The estimated one-rep max lives where rows are folded in memory** (session details, exercise history), not
  in the personal-records projection. Epley's single-rep case and its high-rep cutoff don't survive
  translation to SQL reliably, and **a personal record that reads 3% high is worse than one that isn't shown**.
- ⚠️ **`GetPersonalRecordsAsync` uses plain aggregates only** — `MAX`, `COUNT`, `COALESCE`. The obvious
  "name from the most recent session" is an ordered sub-select inside a grouped projection, which does *not*
  translate to SQL; it was written that way first and replaced with `MAX(ExerciseName)`. This is exactly the
  class of query the InMemory provider passes and SQL Server rejects (ARCHITECTURE §10) — **do not** reach for
  `.OrderBy(...).First()` inside that `GroupBy`.
- **Muscle-group breakdown resolves the group at read time** from the library row, not from the session
  snapshot, so re-classifying an exercise re-colours history. Entries whose exercise was deleted fall under
  `Other` rather than vanishing.
- **Adherence excludes ad-hoc doses from the numerator** — an unplanned dose must not paper over a plan that
  isn't being followed — and applies the quest denominator rule: a day counts only once fully elapsed in the
  user's local calendar (via `LocalDateOn`), or once something was taken that day. `rate: null` means
  "nothing evaluated yet", which is not 0%.
- **`api/workouts/settings`** mirrors `api/finance/settings`, including the rule that changing the weight unit
  *reinterprets* stored weights and never converts them. The allow-list ships in the response so the client
  never hard-codes it.

**Phase 6 — Docs & handoff.** `docs/workouts-module.md` (reference, replacing this file),
`ARCHITECTURE.md` §14, final `docs/workouts-api-schema.ts`, regenerated `docs/swagger.json`, and the
**Polish FE instruction document** in the style of `docs/questy-analityka-frontend.md`.

---

## 6. Persistence hazards — read before writing the first migration

⚠️ **`Exercises` mixes `HasData` system rows with user rows, so it inherits the finance category mine.**
`HasData` writes explicit ids under `IDENTITY_INSERT`, which *raises* the counter but never reserves a range —
which is how a seed migration eventually collides with user data (it cost finance a production `PK` failure on
2026-08-09). **Reseed the `Exercises` identity to 100 000 in the very first migration**: system exercises below
it, user exercises above, and inserting a lower explicit id never drags the counter back down.

Other notes, all confirmed in the finance build:

- A new `IEntityTypeConfiguration` must actually be registered in `AppDbContext` — an unregistered
  configuration **fails silently and voids everything in it**.
- `dotnet` on `PATH` is runtime-only; use `C:\Users\Swida\.dotnet\dotnet.exe`.
- EF design-time commands need `-- production` (`-- development` points at a dead local instance):
  ```
  dotnet ef migrations add <Name>  --project Infrastructure --startup-project Api -- production
  dotnet ef database update        --project Infrastructure --startup-project Api -- production
  ```
- The InMemory provider is exactly where date-boundary and grouped-`SUM` logic passes while real SQL differs —
  which is why the PR projection and adherence math live in pure calculators (ARCHITECTURE §10).

---

## 7. Deferred — designed for, not built

Supersets / dropsets (the reserved `WorkoutSetTypeEnum` slots plus a nullable group key on the routine and
session exercise rows; additive), server-side rest timer, multi-week progressive plans layered *above*
`WorkoutRoutine`, weekday-scoped supplement slots, body measurements & bodyweight tracking, progress photos
(would reuse Cloudinary), supplement reminders (SignalR is already wired), and XP/coin/badge awards for
completed sessions (the event and trigger exist from phase 3; only the strategy is missing).
