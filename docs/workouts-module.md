# Workouts & Supplements — reference

Two sibling feature domains: **training** (exercise library, routines, performed sessions, analytics) and
**supplements** (catalog, schedule, daily checklist, adherence). Both are keyed on **`UserProfile`** (not the
auth `Account`) and follow every convention in [`ARCHITECTURE.md`](../ARCHITECTURE.md).

- **[`ARCHITECTURE.md` §14](../ARCHITECTURE.md#14-workouts--supplements-modules)** — the one-page overview.
- **This file** — the durable detail: model rules, locked decisions *and why they must not be undone*, API
  surface, hazards.
- **`docs/workouts-api-schema.ts`** — the live FE contract (hand-written TS).
  **`docs/swagger.json`** — generated. **`docs/trening-frontend.md`** — the FE team's guide (Polish).
- Supersedes `docs/workouts-module-plan.md` (the phase-by-phase build checklist), deleted 2026-08-15. Recover
  it from git history if you ever need the build log; everything still true was folded into this file.

## Status (2026-08-15)

Core is **done and green** — 208 workouts/supplements tests inside **668** total,
`dotnet test Application.Tests` fully passing. All slices, endpoints and migrations are in place; nothing in
either module is half-built.

Deliberately **not** built (model leaves room): supersets / dropsets (the reserved `WorkoutSetTypeEnum` slots
plus a group key on the routine and session entries — additive), server-side rest timer, multi-week
progressive plans layered *above* `WorkoutRoutine`, weekday-scoped supplement slots, per-user hiding of system
exercises, body measurements & bodyweight tracking, progress photos (would reuse Cloudinary), supplement
reminders (SignalR is already wired), and **XP/coin/badge awards for completed sessions** — the domain event
and badge trigger ship, only the strategy is missing.

## Where the code lives

| Layer | Path |
|---|---|
| Entities | `Domain/Models/{Exercise,WorkoutRoutine,WorkoutRoutineExercise,WorkoutSession,WorkoutSessionExercise,WorkoutSet,Supplement,SupplementScheduleSlot,SupplementIntake,ExerciseBest}.cs` |
| Enums | `Domain/Enums/{ExerciseMetric,MuscleGroup,Equipment,WorkoutSessionStatus,WorkoutSetType,SupplementUnit,SupplementTiming}Enum.cs` + `BadgeTriggerEnum.WorkoutSessionCompleted` |
| Calculators (pure) | `Domain/Calculators/{WorkoutVolumeCalculator,OneRepMaxCalculator,SupplementAdherenceCalculator}.cs` |
| Value objects | `Domain/ValueObjects/{WorkoutLimits,WorkoutSessionTotals,SupplementAdherence,SupportedWeightUnits}.cs` |
| Domain event | `Domain/Events/Workouts/WorkoutSessionCompletedEvent.cs` |
| Repository contracts | `Domain/Interfaces/Repositories/I{Exercise,WorkoutRoutine,WorkoutSession,Supplement,SupplementIntake}Repository.cs` |
| EF configuration + seeding | `Infrastructure/Persistence/Configuration/{Exercise,WorkoutRoutine,WorkoutRoutineExercise,WorkoutSession,WorkoutSessionExercise,WorkoutSet,Supplement,SupplementScheduleSlot,SupplementIntake}Configuration.cs` |
| Repositories | `Infrastructure/Persistence/Repositories/*` (via `UnitOfWork`) |
| Slices | `Application/Workouts/{Exercises,Routines,Sessions,Analytics,Settings}/`, `Application/Supplements/{Catalog,Intakes,Analytics}/` |
| Controllers | `Api/Controllers/{Exercises,WorkoutRoutines,WorkoutSessions,WorkoutAnalytics,WorkoutSettings,Supplements,SupplementAnalytics}Controller.cs` |
| Tests | `Application.Tests/Workouts/{Models,Calculators,Exercises,Routines,Sessions,Analytics,Validators}/`, `Application.Tests/Supplements/{Models,Calculators,Catalog,Intakes,Analytics}/` |

## Data model

**`Exercise`** — the library, a single entity discriminated by `MetricType` (mirrors `Quest` and
`FinanceTransaction`), never a class hierarchy. `UserProfileId == null` marks a seeded system row (like
`Badge` / `FinanceCategory`). `IsArchived` is the retire path.

**`WorkoutRoutine`** + **`WorkoutRoutineExercise`** — a saved template for one runnable workout, with ordered
items carrying optional targets.

**`WorkoutSession`** *(aggregate root)* + **`WorkoutSessionExercise`** + **`WorkoutSet`** — what actually
happened. The entry snapshots `ExerciseName` **and** `MetricType`; the set carries four nullable measurements
plus `Rpe` and `SetType`.

**`Supplement`** + **`SupplementScheduleSlot`** + **`SupplementIntake`** — the "what", the "when and how much",
and the checkbox. An intake may point at no slot (ad-hoc) and optionally at a `WorkoutSession`.

**`ExerciseBest`** — projection type for the grouped personal-records query (the `MonthlyTotal` role).

## Decisions (locked)

### Scope & boundaries

- **The two modules are siblings, not parent/child.** The only link is a nullable `WorkoutSessionId` on an
  intake. This is forced by the requirement that supplements work on a rest day: the plan cannot hang off a
  session. Same posture as "Finance stays independent of Quests".
- **The in-training supplement panel is `GET /supplements/checklist` filtered to
  `PreWorkout`/`PostWorkout`** — that filter is the entire integration. There is no session-scoped supplement
  resource, deliberately.

### Time & units

- **`PerformedOn` and `TakenOn` are `DateOnly` (SQL `date`).** Not negotiable — this is the third time the
  repo meets this trap. `UserProfile.TimeZone` is rewritten on *every token refresh*, so "which training day
  was this" must be a calendar fact. See `FinanceTransaction.OccurredOn` and ARCHITECTURE §6.
- **`StartedAt` / `CompletedAt` / `TakenAt` stay UTC instants** — genuine moments, and what time-of-day
  analytics would read.
- **Defaults for "today" go through `UserProfile.LocalDateOn`** — the codebase's single source of truth for
  the user's calendar day. Used by session start and by the adherence denominator.
- **Weight is `decimal(6,2)`, distance `decimal(8,2)`, stored exactly as typed.** The unit is one setting per
  user (`UserProfile.WeightUnit`, default `kg`) and **changing it never converts history** — identical call to
  `UserProfile.Currency`. A per-set unit column was rejected: it doubles the width of the module's largest
  table for a case that does not occur.
- Use the injected **`IClock`**, never `SystemClock.Instance` (ARCHITECTURE §9).

### Exercises

- **`ExerciseMetricEnum` governs what a set *requires*, never what it forbids.** Logging weight on a
  `Reps` exercise is how a weighted pull-up gets recorded; rejecting it would be restriction for its own sake.
  Extra measurements are stored and returned as given.
- **Metric validation lives in the entity, not in FluentValidation.** It needs the entry's snapshotted metric,
  which only the handler has; restating the metric table in a validator would create a second version of it.
  The entity throws, the middleware makes it a 400.
- **Ownership responses are uniform:** a system row resolves but rejects writes with **403** (not 404 —
  pretending it doesn't exist sends the client hunting a bug that isn't there); another user's row is **404**.
- **Name uniqueness is per user and checked in handlers, not by a database index.** A unique index would also
  block reusing the name of an *archived* exercise, which is legitimate. Shadowing a system exercise's name is
  allowed.
- **Delete is blocked (409) while a routine plans it, and the message names the routines.** It is *never*
  blocked by session history: the handler clears `WorkoutSessionExercise.ExerciseId` and the snapshot carries
  the row. `IsArchived` is what a user reaches for when delete is blocked.
  ⚠️ System exercises cannot be archived — hiding a shared row needs per-user state. Backlog item, not an
  oversight.

### Routines → sessions

- **A routine is a template and a session is a record; the session never writes back.** This is the
  `RecurringTransaction → FinanceTransaction` relationship and inherits its rules: editing a routine does not
  touch past sessions, and deleting one **nulls `RoutineId`** on its sessions (FK stays `Restrict`; the
  handler clears it) rather than deleting them. **There is a regression test for exactly this.**
- **`WorkoutSessionExercise` snapshots `ExerciseName` *and* `MetricType`.** Without it, a rename silently
  rewrites a year of history and a metric change makes old rows unrenderable.
- **The exercise array carries no `order` field — position *is* order**, for routines and sessions alike. A
  separate order value the server has to reconcile against array order is a contradiction waiting to happen,
  and both create and update replace the list wholesale.
- **At most one `InProgress` session per user.** A filtered unique index (`Status = 0`) enforces it; the
  handler turns the violation into a 409 **carrying the active session's id**, so the client can offer
  "resume" without another round-trip. A deliberate step back from the module's general flexibility posture —
  two concurrent sessions is not a feature, it is a state the UI cannot draw.
- **A session may be incomplete and that is legal.** Nothing demands the planned sets were performed.
- **Both logging paths write the same rows and must stay interchangeable.** `PUT /sessions/{id}/log` is **full
  replacement**, which is what makes a retry after a flaky gym connection harmless; the granular
  `POST .../sets` endpoints exist for live logging. An empty array clears the log — legal, so always send the
  whole tree.
- **Every mutating session endpoint returns the full session**, totals included, and re-reads before
  responding (`SessionWriteContext.ReadBackAsync`). One response the client repaints from, byte-identical to
  the next `GET`.
- **Children are resolved inside the loaded session**, which makes ownership checks free: a set id from
  someone else's session isn't in the graph and reads as 404 without a second query.
- **Finish raises `WorkoutSessionCompletedEvent`; abandon deliberately does not** — an abandoned session is
  not an achievement and must never feed gamification. Events are published *before* `SaveChangesAsync` and
  then cleared, matching `DeleteQuestCommandHandler` (no central dispatch — ARCHITECTURE §9).

### Supplements

- **`Supplement` (what) and `SupplementScheduleSlot` (when + how much) are separate.** Magnesium morning
  *and* evening is one supplement with two slots. Duplicate timings are allowed — two morning doses is a real
  pattern.
- **The unit lives on the supplement; slots carry only an amount.** Same inheritance call as a finance
  sub-category inheriting its parent's type: slots that disagreed about the unit would make every aggregate
  over the intakes meaningless.
- **`UNIQUE (ScheduleSlotId, TakenOn)`, filtered to non-null slots.** Double-ticking the same dose on the same
  day is structurally impossible — the `UNIQUE (QuestId, PeriodStart)` lesson, applied up front this time
  rather than after a three-step repair migration. The filter is required: SQL Server treats NULLs as equal,
  which would otherwise collapse every ad-hoc dose on a day into one row.
- **The checkbox is an idempotent `PUT` carrying intent** (`{supplementId, slotId, date, taken}`), not a row
  id. A phone taps fast, offline, and twice. Ticking twice leaves one dose; un-ticking something never ticked
  is a silent no-op.
- **Ad-hoc doses are a separate `POST` and deliberately *not* idempotent.** Repeating an unplanned dose is a
  real event; collapsing two would lose a fact the user chose to record. `DELETE /intakes/{id}` is the undo,
  and the only way to remove an ad-hoc dose (it has no slot to un-tick).
- **`ScheduleSlotId = null` is legal** — an unplanned dose, or one whose slot was later deleted. A model that
  could only record planned doses would punish the user for the exact deviation worth recording.
- **Deleting a supplement is refused (409) once anything is logged against it**, pointing at deactivation.
  Deleting a *slot* succeeds and **nulls its intakes' `ScheduleSlotId`** — `Restrict` alone would block every
  slot the user had ever ticked, i.e. every slot worth deleting. Same mechanism as detaching sessions from a
  deleted routine.
- **Inactive supplements drop out of the checklist's `items` but still appear in `adHoc`**, and an ad-hoc dose
  of an inactive supplement is allowed: deactivating means "off my plan", not "I can never take this".
- **v1 slots are daily.** Weekday scoping is deferred and purely additive; `Timing = PreWorkout` already
  covers "training days only" because the panel filters by timing.

### Analytics

- **Only *completed* sessions count, everywhere, and warm-ups are excluded from every total.** An abandoned
  session is not training you did, and an in-progress one would make today's numbers move under the user as
  they log.
- **Aggregation happens in memory over a range query**, like all five finance analytics handlers; the maths
  lives in pure calculators so it is unit-tested without a database.
- **Personal records are a grouped SQL projection**, not a denormalized column — a fold over all history is a
  few dozen grouped rows at this scale, and denormalizing would buy only drift risk.
- **The estimated one-rep max is *not* in personal records.** Epley's single-rep case and its high-rep cutoff
  do not survive translation to SQL reliably, and a record reading 3% high is worse than one that isn't shown.
  It lives where rows are folded in memory: session details and `exercise-history`.
- **Muscle-group breakdown resolves the group at read time** from the library row, so re-classifying an
  exercise re-colours history. Entries whose exercise was deleted fall under `Other` rather than vanishing.
- **Adherence excludes ad-hoc doses from the numerator** — an unplanned dose must not paper over a plan that
  isn't being followed — and applies the **quest denominator rule**: a day counts only once fully elapsed in
  the user's local calendar, or once something was taken that day. `rate: null` means "nothing evaluated yet",
  which is not 0% and must not be coloured like it. The rate is **not clamped at 100**: extra doses are
  something the user really did.

## API surface

All routes are `[Authorize]`, identity via `User.GetCurrentUserProfileId()`. Enums serialize as strings;
`DateOnly` is `"YYYY-MM-DD"`, `TimeOnly` is `"HH:mm:ss"`.

| Route | Verbs |
|---|---|
| `api/workouts/exercises` | `GET` (system + own; `muscleGroup`/`metricType`/`search`/`includeArchived`) · `POST` · `PUT /{id}` · `PATCH /{id}/archived` · `DELETE /{id}` |
| `api/workouts/routines` | `GET` (`?includeArchived`) · `GET /{id}` · `POST` · `PUT /{id}` (full replacement) · `PATCH /{id}/archived` · `DELETE /{id}` |
| `api/workouts/sessions` | `GET` (`from`/`to`/`status` + paging) · `GET /active` (200, or **204** when nothing runs) · `GET /{id}` · `POST` · `PUT /{id}` (metadata) · `DELETE /{id}` |
| `api/workouts/sessions/{id}/log` | `PUT` (bulk full replacement, idempotent) |
| `api/workouts/sessions/{id}/{finish,abandon}` | `POST` |
| `api/workouts/sessions/{id}/exercises` | `POST` · `DELETE /{entryId}` |
| `api/workouts/sessions/{id}/exercises/{entryId}/sets` | `POST` · `PUT /{setId}` · `DELETE /{setId}` |
| `api/workouts/analytics` | `GET summary` · `exercise-history` · `personal-records` |
| `api/workouts/settings` | `GET` · `PUT weight-unit` |
| `api/supplements` | `GET` (`?includeInactive`) · `POST` · `PUT /{id}` · `PATCH /{id}/active` · `DELETE /{id}` |
| `api/supplements/{id}/slots` | `POST` · `PUT /{slotId}` · `DELETE /{slotId}` (all return the supplement) |
| `api/supplements/checklist` | `GET` (`?date=`, repeatable `?timing=`) |
| `api/supplements/intakes` | `GET` (`from`/`to`) · `PUT` (idempotent toggle) · `POST` (ad-hoc) · `DELETE /{id}` |
| `api/supplements/analytics` | `GET adherence` |

Relational checks (ownership, exercise availability, uniqueness) live in **handlers** → `NotFound` /
`Conflict` / `Forbidden`; validators cover **field** rules only. **No `Program.cs` registration was needed for
any of it** — handlers, validators and Mapster profiles are picked up by the existing assembly scans, all five
repositories are exposed through `IUnitOfWork`, and `IClock` / `IPublisher` were already registered.

## Persistence, migrations & seeding

| Migration | What it does |
|---|---|
| `20260815130138_AddWorkoutsAndSupplementsModules` | All nine tables + `UserProfile.WeightUnit` + the identity reseed |
| `20260815131826_SeedSystemExercises` | The 101-exercise starter library |

Both were applied to production on 2026-08-15.

⚠️ **`Exercises` mixes `HasData` system rows with user rows on one IDENTITY sequence** — the arrangement that
broke `PK_FinanceCategories` in production on 2026-08-09. `HasData` writes explicit ids under
`IDENTITY_INSERT`, which *raises* the counter but never reserves a range. **The first migration reseeds the
identity to 100 000**: system exercises below, user exercises above. Never seed an id at or above 100 000.

**The system library is 101 Polish-named exercises**, calisthenics-first with enough barbell/dumbbell/machine
work to log an ordinary gym session. No i18n on the backend — the same call the finance taxonomy made. It is a
starting point, not a canon: users add their own freely and archive what they don't do.
**Source of truth is `ExerciseConfiguration.SeedSystemExercises`** — there is deliberately no companion
`.txt`, so it cannot drift. Id blocks, which **must stay stable**:

| Block | Group | Count |
|---|---|---|
| 1–99 | Klatka piersiowa | 16 |
| 100–199 | Plecy | 16 |
| 200–299 | Barki | 9 |
| 300–399 | Ramiona | 12 |
| 400–499 | Core | 14 |
| 500–599 | Nogi | 18 |
| 600–699 | Całe ciało | 7 |
| 700–799 | Cardio | 9 |

Number new rows inside the matching block.

Only **two** unique indexes exist, both load-bearing: `(ScheduleSlotId, TakenOn)` filtered to non-null slots,
and one `InProgress` session per user filtered on `Status = 0`. Everything else — including all name
uniqueness — is a handler check, on purpose (see the decisions above).

**Environment notes:**

- `dotnet` on `PATH` is runtime-only — use `C:\Users\Swida\.dotnet\dotnet.exe`, and set **both** `DOTNET_ROOT`
  and prepend that directory to `PATH` before `dotnet ef`, or it fails with "the application 'msbuild' does
  not exist".
- EF design-time commands need `-- production`:
  ```
  dotnet ef migrations add <Name>  --project Infrastructure --startup-project Api -- production
  dotnet ef database update        --project Infrastructure --startup-project Api -- production
  ```
- A new `IEntityTypeConfiguration` must be registered in `AppDbContext.OnModelCreating` — there is no
  assembly scan, and an unregistered configuration **fails silently and voids everything in it**.
- Regenerating `docs/swagger.json`: `dotnet run --project Api --launch-profile Production_http`, then
  `GET http://localhost:5168/swagger/v1/swagger.json`. Swagger is registered unconditionally, so the
  Production profile works and points at the real database; startup fires the hosted `StartupTask`s.
  Repo `docs/*.json` use CRLF.

## Tests

208 tests across `Application.Tests/Workouts/` and `Application.Tests/Supplements/`, split the way
ARCHITECTURE §10 recommends:

- **Pure, no DB:** `Models/` (entity invariants, the metric contract, session lifecycle) and `Calculators/`
  (volume, Epley, the adherence denominator).
- **Handlers:** CRUD + ownership per slice, the delete-blocking rules, bulk-log idempotency, set renumbering,
  and the gamification hook being published on finish but not on abandon.
- **Regressions worth knowing about:** editing or deleting a routine never reaches a session already performed
  from it; deleting an exercise keeps session history via the snapshot; deleting a slot keeps the doses taken
  against it; toggling the same slot twice on the same day is a no-op.
- **Validators get their own tests** — handlers are `new`-ed up directly, so `ValidationBehavior` never runs.

⚠️ The InMemory provider is exactly where a grouped projection passes while real SQL rejects it. The personal
records query was first written with `.OrderBy(...).First()` inside its `GroupBy` — untranslatable, and the
tests were green. It now uses plain aggregates only. **Do not reach for ordered sub-selects inside that
grouping**; if repository-level coverage is ever added, use SQLite in-memory.

## Front-end & contracts

Front-end work lives in the FE repo (`GoodieHabbi-FN`), not here. The backend contracts
`docs/workouts-api-schema.ts` (the live, hand-written one), `docs/swagger.json` (generated) and
`docs/trening-frontend.md` (the FE guide, Polish) are current as of 2026-08-15.
