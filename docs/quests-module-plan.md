# Quests — flexible recurrence redesign (plan)

> **Status: phase 1 core built and verified against production data (2026-09-12).** 736 tests green.
> **Step 1 of the migration is applied to production; step 2 is deliberately NOT** — the legacy columns
> still hold their original values, which is what keeps a rollback possible until the FE ships.
> Instead of the full route shim (§8), `GET /quests?legacyType=` reproduces the old per-type screens.
> Still open: the FE contract docs (`quests-api-schema.ts`, the Polish guide, ARCHITECTURE §6), and step 2.
> The seven questions are settled in §13.
> When this ships, fold the durable parts into a `docs/quests-module.md` reference + `ARCHITECTURE.md` §6 and
> delete this file, the way `FINANCE_MODULE_PLAN.md` and `workouts-module-plan.md` were handled.

---

## 1. Why: what the current model can't express

| Scenario | Today |
|---|---|
| Brush teeth 2× a day | Not possible. An occurrence is a boolean, and `Quest.Complete` returns early once `IsCompleted` is set. |
| Exercise at least 2× a week, any days | Not possible. "Weekly" means "on these weekdays", and each weekday is its own single-day period. |
| Every other day | Not possible. |
| Drink 2 L of water / read 30 pages | Not possible. |
| A seasonal habit that comes back every year | Seasonal quests aren't repeatable, and `IsCompleted` is never reset, so the quest stays done forever after year one. |

Three structural causes:

1. **The recurrence shape is the type enum.** Each `QuestTypeEnum` value brings its own command, request, handler,
   validator, DTO and controller region, for both create and update. It also brings a satellite table
   (`WeeklyQuest_Day`, `MonthlyQuest_Days`, `SeasonalQuest_Season`) and a branch in every switch:
   `QuestWindowCalculator`, `NextResetDateCalculator`, `QuestRepository` includes, `QuestMapper`, `UserProfile`
   counters and the badge strategies. Adding "N× per week" today means touching all of them.
2. **An occurrence is a boolean.** There is no target and no count.
3. **"Is it done" is a persisted flag.** `Quest.IsCompleted` is cleared by a background job at `NextResetAt`, rather
   than derived from the period the user is in.

---

## 2. Findings in the current code (independent of the redesign)

- **Every quest job runs only when the process starts.** `StartupTask` calls `ExecuteAsync` from `StartAsync`,
  once. `ResetQuestsTask` is the only code that clears `IsCompleted`, and both `Quest.Complete` and
  `UpdateQuestCompletionCommandHandler` return early while the flag is set, so a daily quest completed yesterday
  cannot be completed today until the process restarts. **On this hosting that restart happens constantly** (§9),
  so this is fragile rather than broken — it would only bite if the process stayed warm across the user's local
  midnight. Same for stale `QuestStatistics`, goal expiry and recurring finance transactions.
- ⚠️ **Coins can be farmed by toggling completion.** `UserProfile.ApplyQuestCompletionRewards` runs
  `Coins += 10` outside the `shouldAssignRewards` branch, and `RevertQuestCompletion` never takes the coins back.
  Complete → uncomplete → complete gives +10 coins on every cycle. XP is protected by the same-day check; coins
  are not.
- **Goals are achieved by the first completion, whatever the goal type.** `Quest.Complete` marks every active
  goal as achieved, so a Yearly goal on a daily habit is achieved on day one.
- **The "on-time" XP bonus is always granted for repeatable quests** that have an end date (`today <= EndDate`
  holds for as long as the quest is active).
- `GetQuestsEligibleForGoalQueryHandler` uses the UTC date instead of `UserProfile.LocalDateOn`.
- `GetQuestAnalyticsQueryHandler` loads the quest twice: once with badges, once more just for statistics.
- `SystemClock.Instance` is still used directly in the create, update, completion, reset, generation and
  recalculation handlers (ARCHITECTURE §9).
- Season boundaries are fixed to the northern hemisphere's astronomical dates.

---

## 3. Core idea

Today one enum and one boolean hold three separate concepts. Split them:

1. **Schedule**: which calendar periods exist.
2. **Target**: what counts as success within one period.
3. **Completion log**: what the user actually did, one row per tap.

**The period is the unit of accountability.** Streaks, completion rate, rewards and goals are all judged per
period. A period is the existing `QuestOccurrence`: it stays, and gains a target and a progress value.

| Scenario | Unit | Interval | Period filter | Target |
|---|---|---|---|---|
| Brush teeth 2×/day | Day | 1 | – | ≥ 2 |
| Gym Mon/Wed/Fri *(today's Weekly)* | Day | 1 | weekdays = Mon, Wed, Fri | ≥ 1 |
| Exercise at least 2×/week, any days | Week | 1 | – | ≥ 2, max 1/day |
| Every other day | Day | 2 | – | ≥ 1 |
| Pay rent between the 1st and 5th *(today's Monthly)* | Month | 1 | days 1–5 | ≥ 1 |
| Read on 20 days a month | Month | 1 | – | ≥ 20, max 1/day |
| Drink 2 L | Day | 1 | – | ≥ 2 `L` |
| Winter hike each year *(today's Seasonal)* | Year | 1 | Dec 21 – Mar 20 | ≥ 1 |
| Renew passport *(today's OneTime)* | None | – | StartDate..EndDate | ≥ 1 |
| At most 2 coffees/day *(phase 2)* | Day | 1 | – | ≤ 2 |

- **"Sometimes 2, sometimes 3 workouts a week"** is *at least 2 per week*. A 3-workout week shows `3/2 ✓`: the
  extra workout is recorded and counted in analytics, but earns no extra reward. A stretch-target bonus could be
  added in phase 2.
- **Brushing teeth: one quest with target 2, not two quests.** The streak then means "both done", the rate
  reflects it, and a half-done day is visible as progress `1/2`. The per-brushing analytics that separate quests
  would give come from the completion log instead (the time-of-day breakdown, §6). If you later want named slots
  with reminders ("morning 08:00 / evening 21:30"), that is phase 2 and still one quest. Separate quests remain
  the right call for genuinely different habits.
- **Rolling windows** ("2 in any 7 days") are rejected. They don't map to calendar cells, so the calendar,
  streaks and the `(QuestId, PeriodStart)` identity would all stop making sense.

---

## 4. Domain model

### `Quest` changes

```
Quest
  Title, Description, Emoji, Priority, Difficulty, Labels, StartDate, EndDate, ScheduledTime   (unchanged)
  Schedule : QuestSchedule          NEW  owned value object → columns on Quests
  Target   : QuestTarget            NEW  owned value object → columns on Quests
  LastCompletedAt, WasEverCompleted       kept (denormalized)
  QuestType, IsCompleted, NextResetAt     REMOVED (derived, see §5)
  WeeklyQuest_Days, MonthlyQuest_Days, SeasonalQuest_Season   REMOVED (folded into Schedule)
```

```csharp
public sealed record QuestSchedule(
    PeriodUnitEnum Unit,          // None | Day | Week | Month | Year
    int Interval,                 // every N units, >= 1; anchored at StartDate ?? creation local date
    WeekdayFlags? Weekdays,       // Day only: which weekdays are periods. [Flags], one int column
    int? MonthWindowStartDay,     // Month only: 1..31, clamped per month (today's MonthlyQuest_Days)
    int? MonthWindowEndDay,
    int? YearWindowStart,         // Year only: MMDD, e.g. 1221. May wrap past year end
    int? YearWindowEnd);          // e.g. 0320

public sealed record QuestTarget(
    decimal Amount,               // default 1; must be a whole number when Unit is null
    string? Unit,                 // null = "times"; otherwise a free label: "L", "pages", "min" (<= 20 chars)
    TargetModeEnum Mode,          // AtLeast (build a habit) | AtMost (limit a habit, validator rejects until phase 2)
    int? MaxCompletionsPerDay);   // Week/Month/Year only; null = unlimited; UI default 1
```

One pure `QuestPeriodCalculator` replaces `QuestWindowCalculator`, `NextResetDateCalculator`, the per-type
`WHERE` in `GetActiveQuestsForDisplayAsync` and `SeasonHelper.GetCurrentSeason`. Its entry points are
`PeriodCovering(date)`, `PeriodsBetween(from, to)` and later `ToRRule()`. A user has tens of quests, not
thousands, so the active-quest query loads the user's quests whose date range is active and filters them in memory
with the same calculator. That keeps a single implementation of the scheduling rules instead of a C# copy and a
SQL copy.

### `QuestOccurrence`: the period (kept, extended)

```
QuestOccurrence
  PeriodStart, PeriodEnd            unchanged; UNIQUE (QuestId, PeriodStart) stays
  TargetAmount    decimal(9,2)      NEW  snapshot of Quest.Target.Amount (prorated, §5)
  Progress        decimal(9,2)      NEW  Σ completion amounts (denormalized, maintained by the domain)
  CompletedAt     DateTime?         now: the instant the target was reached (was: the tap)
  IsBackfilled    bool              unchanged
  RewardGrantedAt DateTime?         NEW  plus XpAwarded / CoinsAwarded (§7)
  SkippedAt, SkipReason             NEW  phase 2
  RowVersion                        NEW  concurrency token
  WasCompleted                      REMOVED → derived as CompletedAt != null
  Completions : ICollection<QuestCompletion>
```

- **The target is snapshotted.** Editing "2×/week" to "3×/week" must not turn last month's successes into
  failures. This is the same template-vs-record rule as `RecurringTransaction → FinanceTransaction` and
  `WorkoutRoutine → WorkoutSession`. It is also why periods stay materialized instead of being derived from the
  rule when read: the rows are the historical record of what was asked of the user.
- **`RowVersion` is required.** `Progress` is denormalized, so without it two concurrent taps on a `1/2` period
  would both read 1, both reach the target and both grant the reward.

### `QuestCompletion` (new): the log

```
QuestCompletion
  Id
  QuestId, UserProfileId       owner denormalized, following the SupplementIntake precedent
  OccurrenceId     int?         null = off-schedule completion
  CompletedOn      DateOnly     the local day the completion counts for (a calendar fact)
  CompletedAt      DateTime     UTC instant of the tap
  LocalTime        TimeOnly?    local wall-clock time at the tap, snapshotted (feeds byHourOfDay)
  Amount           decimal(9,2) default 1
  IsBackfilled     bool
  Source           enum         App | Widget | Calendar | Import
  ClientRequestId  Guid?        filtered UNIQUE (QuestId, ClientRequestId)
  Note             string?      <= 250, phase 2
```

- **Off-schedule completions** (`OccurrenceId = null`) use the `SupplementIntake` ad-hoc argument: a model that
  can only record planned activity punishes the deviation most worth recording. A bonus Tuesday workout on a
  Mon/Wed/Fri quest counts toward totals and time analytics. It never belongs to a period, never causes a
  failure, and earns no reward.
- **`LocalTime` is snapshotted for the same reason `CompletedOn` is.** Computing it later from `CompletedAt` and
  the profile's current timezone would shift the hour of every tap made while travelling.
- **`ClientRequestId`** makes an accidental double-tap, an offline retry and a future calendar sync all
  idempotent. This matters more once a second tap is legitimate: on a target-2 quest, a double-tap would
  otherwise complete the day by accident.
- Indexes: `(QuestId, CompletedOn)` and `(UserProfileId, CompletedOn)`.

---

## 5. Rules

### Periods

- **Generation** is unchanged in spirit: catch up from `lastPeriodEnd + 1` (or `StartDate ?? creation date`) to
  local today. Only the calculator underneath changes.
- **Partial periods are clipped and prorated.** When `StartDate`/`EndDate` cuts through a Week, Month or Year
  period, the period is clipped to the active days and its target becomes `ceil(target × activeDays / fullDays)`.
  Otherwise a "3×/week" quest created on a Saturday starts with a guaranteed failure.
- **Week start** is a user setting, `UserProfile.WeekStartsOn`, defaulting to Monday to match the ISO buckets the
  analytics already use. A change affects only periods generated afterwards. The transition period runs from
  `lastPeriodEnd + 1` to the next boundary and is prorated like any partial period.
- **Interval** is anchored on `StartDate ?? creation local date` and aligned to unit boundaries.
- **Schedule edits.** Elapsed periods and any period with completions are never touched. Pending periods from today
  onward that have no completions are deleted and regenerated. If the unit changed, the first new-unit period
  starts after the last kept period, clipped and prorated. This generalizes what `UpdateWeeklyQuestCommandHandler`
  already does for removed weekdays.
- **Target-only edits** re-snapshot the current pending period and future ones; elapsed periods keep their
  snapshot.

### Completing

`Quest.AddCompletion(amount, completedOn?, clientRequestId?, nowUtc)`:

1. `completedOn` defaults to local today. Past days are allowed within the grace window; future days → 400. A
   repeated `ClientRequestId` returns the existing completion.
2. Resolve the period covering `completedOn`, materializing missing periods as today. If there is none, record an
   off-schedule completion.
3. Enforce `MaxCompletionsPerDay` against completions sharing that `CompletedOn` → 409. A sanity cap per period
   (e.g. 10× target) → 409. Otherwise going over the target is allowed and recorded.
4. `Progress += amount`. When progress reaches the target for the first time: set `CompletedAt` (and
   `IsBackfilled` if the period has elapsed), grant the reward once (§7), and check goals.

`Quest.RemoveCompletion(id)` subtracts the amount. If progress drops below the target, `CompletedAt` is cleared.
The reward is not revoked and is never granted twice.

- **This replaces the implicit fallback to the previous period** in `GetOrCreateCurrentOccurrence`. Once multiple
  completions are normal, silently crediting yesterday when the user taps on an unscheduled day is surprising. The
  client sends `completedOn` explicitly ("I did it yesterday").
- **`isCompleted` becomes derived.** It is true when the period covering today has a `CompletedAt` (for one-time
  quests, their single period). There is no flag, no `NextResetAt` and no reset job, so the startup-only failure
  mode from §2 disappears rather than getting patched.

### Catch-up: backfilling a forgotten tap

People do the habit and forget to tap. Today this is close to impossible to fix: completion is a flag on the
quest, and the period is resolved implicitly from "now", so there is no way to say "this was Tuesday". The
completion log removes the obstacle rather than working around it — a completion carries its own `CompletedOn`,
and period rows already exist whether or not anything was recorded against them. **Daily quests stop being the
hard case**: "yesterday's period" is just the period covering yesterday's date.

**The window.** `completedOn` must be within `GraceDays` (**2**) of local today — so today, yesterday and the day
before. One rule, applied identically to every unit, from options rather than per quest.

For Week, Month and Year periods the rule falls out for free. While such a period is still running there is no
backfill at all — you are completing a pending period, as normal. The window only bites in the days right after a
period ends, which is exactly when someone would remember. Its cost is that a monthly period cannot be corrected
three weeks later; that is deliberate. Rewriting old periods would make every streak and rate untrustworthy, and
inventing history is precisely the damage the last occurrence migration had to repair. If it turns out to be
needed, phase 2 can add an explicit, clearly-labelled "edit history" action that is excluded from streaks.

**How it counts.** Inside the window a backfill is a *correction of a recording error*, so it counts fully: the
period flips from Missed or Partial to Completed, streaks and rates recompute (every number is derived from the
rows), and the reward is granted — once, capped at what a timely completion would have earned. `IsBackfilled` is
set on both the completion and the period, and is already exposed on calendar cells, so the UI can shade it
differently if you want to. `MaxCompletionsPerDay` is checked against `CompletedOn`, so backfilling cannot be used
to dodge it.

**The surface, so the client doesn't have to invent one:**

```
GET /api/quests/catch-up
→ { graceDays: 2,
    days: [ { date: "2026-09-10",
              quests: [ { questId, title, emoji, periodStart, periodEnd,
                          progress, target, outcome } ] } ] }
```

It lists only periods where a tap would still change the outcome — elapsed, inside the window, Missed or Partial.
Completed periods never appear, so an empty `days` array means "nothing to ask about" and the card stays hidden.
On app open it renders as one dismissible card ("3 habits unticked from the last 2 days") expanding into a
day-grouped checklist. **Ticking an item calls the ordinary completions endpoint with `completedOn` set** — there
is no separate write path to keep consistent. Dismissal is client-side in v1.

### Outcomes

| Outcome | Rule | In rate denominator | Streak |
|---|---|---|---|
| Completed | progress ≥ target | yes | +1 |
| Partial *(new)* | elapsed, 0 < progress < target | yes (a miss) | breaks |
| Missed | elapsed, progress = 0 | yes | breaks |
| Pending | not elapsed, not completed | no | ignored |
| Skipped *(phase 2)* | user skipped or excused it | no | ignored, doesn't break |

The existing denominator rule (only elapsed or completed periods are evaluated) is unchanged. `Partial` exists so
the calendar can show "1 of 2" instead of plain red; it still counts as a miss. `AtMost` (phase 2) inverts the
rules: the period fails as soon as progress exceeds the target, and succeeds when it elapses without doing so.

---

## 6. Analytics

Additive where possible:

- **`progressRate`** is added to `QuestAnalyticsSummary`: `Σ min(progress, target) / Σ target` over evaluated
  periods. This is the partial-credit number (teeth: 40% of days fully done, 75% of brushings done).
  `completionRate` keeps its strict meaning.
- `partialPeriods`, `totalCompletions` and **`streakUnit`** are also added. A streak of "5" on a 2×/week habit
  means five weeks, and the UI needs to know that.
- `QuestCalendarEntry` gains `progress`, `target` and the `Partial` outcome.
- **`byWeekday` is computed from the completion log** (`CompletedOn`) instead of single-day periods only. It then
  works for every unit ("which days do I actually train" on a 3×/week habit). Today it is empty for monthly
  quests.
- **New: `byHourOfDay`**, computed from `QuestCompletion.LocalTime`. This is the morning-vs-evening brushing
  answer.
- **`/active` gets in-period hints**: `progress`, `target`, `periodEnd`, `remainingDays` and `isAtRisk`
  (more completions still needed than days left under `MaxCompletionsPerDay`), e.g. "2 more workouts, 2 days
  left".
- **The overview `dailyCompletionRate` changes (FE-visible).** It currently spreads every multi-day period across
  each of its days, so one missed week-target period would paint seven red days. The daily series will use
  Day-unit periods only, and a separate `periodic` summary will cover Week, Month and Year periods.
- The `QuestStatistics` cache gains `TotalCompletions` and `PartialCount`, and is kept fresh by the periodic job
  (§9) instead of a startup job.

---

## 7. Rewards

**Today:** each tap gives 10 XP plus difficulty, priority and "on-time" modifiers. XP is gated to once per local
day through `LastCompletedAt`; coins are +10 on every tap (farmable, §2).

**Proposal:**

- **Reward the period when its target is reached, not each tap.** Otherwise a "10 glasses of water" habit out-earns
  a daily gym habit tenfold.
- **Value** = base × difficulty/priority modifiers × a **period weight** (roughly Day 1, Week 3, Month 8; tunable
  through options) × an optional capped streak multiplier.
- **Granted once per period and recorded on the occurrence** (`RewardGrantedAt`, `XpAwarded`, `CoinsAwarded`).
  Removing a completion doesn't claw the reward back (no level-downs), and it can't be earned again. Because the
  amounts are recorded, a revocable policy could be introduced later without a migration.
- Off-schedule completions earn nothing.
- **Profile counters** (`CompletedQuests`, `CompletedDaily/Weekly/MonthlyQuests`) count completed periods, grouped
  by unit. `BalancedHero` keeps working.
- **Badge strategies switch from `QuestType` to `Schedule.Unit`.** `CompleteDailySeven` becomes Unit = Day with
  `LongestStreak ≥ 7`; `CompleteMonthlyTwelve` becomes Unit = Month. A weekly-streak badge is a natural addition.
- **Goals** are achieved when one of the quest's periods reaches its target inside the goal window, not on the
  first tap. A deeper goal redesign ("N successful periods in the window") is deferred.

---

## 8. API

One request shape replaces five create/update pairs, and `{questType}` disappears from the routes.

```
POST   /api/quests                                    create (schedule + target in the body)
PUT    /api/quests/{id}                               update
GET    /api/quests/{id}
GET    /api/quests?unit=Week                          replaces GET /{questType}
DELETE /api/quests/{id}
GET    /api/quests/active                             today's list, with currentPeriod

POST   /api/quests/{id}/completions                   { amount?, completedOn?, clientRequestId? } → completion + currentPeriod
DELETE /api/quests/{id}/completions/{completionId}    undo
GET    /api/quests/{id}/completions?from&to

PUT    /api/quests/{id}/periods/{periodStart}/skip    phase 2, idempotent
```

```jsonc
// POST /api/quests
{
  "title": "Exercise",
  "difficulty": "Hard",
  "startDate": "2026-09-14",
  "schedule": { "unit": "Week", "interval": 1 },
  "target":   { "amount": 2, "mode": "AtLeast", "maxCompletionsPerDay": 1 }
}
```

- **Response shape.** `QuestDetailsDto` stops being polymorphic. It carries `schedule`, `target`,
  `currentPeriod { start, end, progress, target, outcome, remainingDays, isAtRisk }`, `statistics` (null for
  one-time quests) and the derived `isCompleted`.
- **Compatibility shim for one release**, because installed mobile builds still call the old routes. Each old
  route becomes a thin mapper onto the new commands:
  - `POST/PUT /api/quests/{kind}` builds a schedule from the old fields.
  - `PATCH /{questType}/{id}/completion { isCompleted }`: `true` adds completions until the target is reached;
    `false` removes the current period's completions.
  - `GET /{questType}`, `GET /{questType}/{id}` and `DELETE /{questType}/{id}` become route aliases.
  - Responses include a derived legacy `questType`. This is best-effort: shapes the old app can't represent
    (Week unit, targets above 1) fall back to the nearest legacy type, so a forced minimum app version is worth
    considering if one exists.

---

## 9. Background work, and the hosting constraint

**Hosting (stated 2026-09-12): shared IIS, app-pool idle timeout 15 minutes, two users, no store release
planned.** There is no always-on process, so `BackgroundService` + `PeriodicTimer`, Hangfire and Quartz are all
unreliable here — every one of them needs a process that is still alive when the timer fires. **My earlier
recommendation to convert the startup tasks to periodic services is withdrawn.**

**Correction to §2.** With a 15-minute idle timeout the pool starts on the first request after any gap, so
`StartupTask` is effectively an on-demand scheduler, and the reset job runs many times a day. The "a daily quest
completed yesterday can't be completed today" failure needs the process to stay warm across the user's local
midnight — which with two users essentially never happens. It is fragile in principle, not broken in practice,
and the original decision was the right one for this environment. I overstated it.

**The redesign removes the need for scheduling rather than asking for more of it:**

| Job today | After phase 1 |
|---|---|
| `ResetQuestsTask` | **Deleted.** `isCompleted` is derived from the current period; there is nothing to reset. |
| `ProcessOccurrencesTask` | On demand. Periods are materialized by the first completion, and by the maintenance pass below. |
| `RecalculateRepeatableQuestStatisticsTask` | Recalculated whenever a quest's periods change. |
| `ExpireGoalsTask` | Derived from `EndsAt`; the flag becomes a denormalization the pass keeps in step. |

**The maintenance pass.** One idempotent, per-user pass guarded by a `UserProfile.MaintainedThrough` (`DateOnly`)
watermark: materialize missing periods up to local today, recalculate statistics, expire goals. It runs at most
once per user per local day, is bounded by one user's quests, and is invoked from `GET /api/quests/active` — the
call the app already makes on open. That is a deliberate exception to "reads never write", and this hosting is
the justification; it is guarded by the watermark and committed separately from the read. The existing startup
tasks stay as a free safety net.

This also means **nothing in phase 1 depends on a scheduler existing**, and the same watermark pattern would fix
`GenerateRecurringTransactionsTask` in the finance module.

**What genuinely needs a scheduler**, whenever you get there: reminders and push notifications (phase 2), which
must fire while the app is closed, and calendar reconciliation (phase 3, though writes can piggyback on quest
edits). Options, cheapest first:

1. **An external cron calling a secured maintenance endpoint** (`POST /api/maintenance/run` behind a shared
   secret). A scheduled GitHub Actions workflow is free, and the repo is already on GitHub; cron-job.org or any
   always-on machine works equally well. Needs nothing from the hosting provider — worth confirming only that
   inbound requests aren't rate-limited oddly. **This is what I would do.**
2. **Ask the provider** whether the app pool can have `startMode="AlwaysRunning"`, idle timeout 0 and IIS
   Application Initialization. Shared hosts often refuse, and it is worth knowing before designing around it.
3. **Move the API to a host with an always-on tier** (Azure App Service with Always On, Fly.io, Railway, a small
   VPS) — the right answer around a store release, not before.

Note that option 1 also makes the maintenance pass reliable for users who have not opened the app, which matters
for "you missed 3 days" style notifications later.

---


## 10. Google Calendar: what matters now

**Decided (2026-09-11): one-way export of quests as calendar events. Completion stays in the app.** That is the
version worth building — it is the part that earns its keep (seeing habits next to meetings when planning a day),
and it needs no round-trip protocol, no convention for "done", and no conflict resolution.

Sync itself is phase 3, but four things in phase 1 keep it cheap:

1. **The schedule is RRULE-compatible, not stored as RRULE.**
   - Day + weekdays → `FREQ=WEEKLY;BYDAY=MO,WE,FR`
   - Day + interval → `FREQ=DAILY;INTERVAL=2`
   - Month window → a monthly multi-day all-day event
   - Year window → a yearly event

   Structured columns stay queryable and easy to validate, and `ToRRule()` is a pure function. "N× per week, any
   day" has no RRULE equivalent because it is a target, not an event; it exports as one all-day event spanning
   the period, titled with its progress ("Exercise 1/2").
2. **Periods have a stable identity**, `(QuestId, PeriodStart)`, which maps onto recurring-event instance ids.
   Updating a single instance (a ✅ in the title once the target is reached) needs no extra table.
3. **`Source` and `ClientRequestId` on completions.** Not needed for export, but they are one column each, and
   without them an import added later cannot avoid double-counting taps already made in the app.
4. **`ScheduledTime` needs a duration** (`DurationMinutes?`) to become a timed rather than all-day event. Several
   times per day means the phase-2 time slots, with one event series per slot.

Export still needs per-user OAuth tokens, a sync-state table (Google calendar id, event ids, sync token) and a
real scheduler — the startup-task pattern cannot do it. That is the bulk of phase 3's cost, and it is unavoidable
for any calendar integration.

**If completing from the calendar is ever wanted**, Google Tasks is the honest path: unlike events, tasks have a
real completed state that can be ticked inside the Calendar UI. The open question is recurrence — the Tasks API
exposes far less of it than the Calendar API does, so a spike would need to confirm whether the schedules above
survive the trip, or whether each period has to be pushed as its own task. Events cannot be used for this: they
have no done state, so "completed" would have to be smuggled into a field the user edits (colour, title), which
is fragile and unexplainable in a UI.

---

## 11. Migration (data preserved)

This follows the `QuestCalendarPeriods_Step1_AddColumns` → backfill → `Step2_Finalize` precedent. This time the
backfill needs no timezone conversion, so it can be plain SQL inside the migration. The C# backfill task from last
time is not needed.

**Step 1 (additive).**
- On `Quests`: nullable schedule and target columns.
- On `QuestOccurrences`: `TargetAmount` (default 1), `Progress` (default 0), `RewardGrantedAt`, `XpAwarded`,
  `CoinsAwarded`, `RowVersion`.
- A new `QuestCompletions` table.
- `UserProfiles.WeekStartsOn` (default Monday).

**Backfill (SQL, in step 1).**

| Legacy type | New schedule |
|---|---|
| OneTime | Unit None. One period from `StartDate ?? CAST(CreatedAt AS date)` to `EndDate ?? 9999-12-31`, completed at `LastCompletedAt` if `IsCompleted`. |
| Daily | Unit Day, interval 1 |
| Weekly | Unit Day, `Weekdays` = bitwise OR over `WeeklyQuest_Days` |
| Monthly | Unit Month, window from `MonthlyQuest_Days` |
| Seasonal | Unit Year, window from the `SeasonHelper` boundaries. Materialize only the period for the season covering today (completed if `IsCompleted`). **Earlier seasons are not generated.** That history never existed, and inventing misses is exactly the damage the last migration had to repair. |

Occurrence rows:
- `TargetAmount = 1`, `Progress = WasCompleted ? 1 : 0`.
- `RewardGrantedAt = CompletedAt` on completed rows, so they aren't rewarded a second time.

Completion rows, one per completed occurrence:
- `Amount = 1`, `CompletedAt` copied, `Source = App`.
- `CompletedOn = PeriodStart` for single-day periods. This is exact, including backfilled completions, because
  `CompletedOn` is the day the completion counts for.
- For monthly periods, `CompletedOn` is the UTC date of `CompletedAt` clamped into the period. It may be off by a
  day, but always lands inside the correct period.
- `LocalTime = NULL`. Legacy taps are excluded from `byHourOfDay`, which is the price of skipping a C# timezone
  backfill.

**Step 2 (finalize).** Hard-fails if any quest still has a NULL `Schedule_Unit`. Then:
- adds NOT NULL constraints;
- drops `QuestType`, `IsCompleted`, `NextResetAt` and `WasCompleted`;
- drops `WeeklyQuest_Days`, `MonthlyQuest_Days` and `SeasonalQuest_Season`.

`QuestStatistics` is recomputed on the job's first run.

---

## 12. Phases

**Phase 0: dropped as a separate step.** Of its four items, phase 1 subsumes three — it rewrites the reward path
(coin farming), rewrites those handlers (`IClock`), and removes the jobs rather than making them periodic, which
§9 now rules out anyway. The one survivor, the UTC-vs-local date in eligible-for-goal, rides along inside phase 1.
Nothing here blocks phase 1, and fixing code that phase 1 deletes is throwaway work.

**Phase 1: core**
- Schedule, target, completion log, and period target/progress (+ calculator).
- The per-user maintenance pass and its watermark (§9); delete `ResetQuestsTask` and `NextResetAt`.
- Migration (§11).
- New API plus the compatibility shim.
- Catch-up: the 2-day grace window and `GET /api/quests/catch-up` (§5). Ships with the core rather than after it —
  it is the payoff the completion log exists for, and it is a few lines once completions carry their own date.
- Analytics additions (§6), per-period rewards (§7), goals achieved on target, badges re-keyed to unit.
- Tests.
- Docs: `ARCHITECTURE.md` §6, `quests-api-schema.ts`, and a Polish FE guide in the style of
  `questy-analityka-frontend.md`.

**Phase 2: flexibility on top of the core**
- Skip/excuse days, plus a **"streak freeze" consumable** (the shop already has consumables and
  `ActiveUserEffect`).
- `AtMost` limit habits.
- Named time slots with reminders over the existing notifications/SignalR.
- Stretch-target bonus, per-completion notes, amount units in the UI.

**Phase 3: Google Calendar (one-way export)**
- OAuth per user, a sync-state table, and a real scheduler.
- Export through RRULE; per-instance titles carrying progress.
- Completing from the calendar is explicitly *not* in scope; if it is ever wanted it means Google Tasks and a
  spike first (§10).

### Tests

- **Pure calculator tests:**
  - period generation across unit × interval × filters × proration × week-start transitions;
  - outcomes and `progressRate`;
  - streaks by unit;
  - reward granted once;
  - `MaxCompletionsPerDay`;
  - `ClientRequestId` idempotency;
  - catch-up: the window boundary (exactly `GraceDays` old vs. one day older), a backfill flipping a Missed period
    and repairing the streak, and the reward still granted only once.
- **Relational tests (SQLite or Testcontainers)** for the new queries, the filtered unique index and `RowVersion`.
  InMemory ignores all three (ARCHITECTURE §10, §14).

---

## 13. Decisions (settled 2026-09-11)

| # | Question | Decision |
|---|---|---|
| 1 | Rewards when a completion is removed | **Granted once per period, never clawed back, never re-granted.** Nothing is enforced against the user; mistakes happen, and the cost of policing remove/re-add outweighs any benefit. |
| 2 | One-time quests | **A single period.** Fits the rest of the model, and gives "read 3 chapters by Friday" (target 3) for free. |
| 3 | API evolution | **New routes plus a one-release compatibility shim**, not `/api/v2`. |
| 4 | Week start | **Per-user setting, default Monday.** |
| 5 | Taps on unscheduled days | **Recorded as off-schedule completions, no reward** — never rejected. |
| 6 | Profile counters by unit | **Accepted.** Counters key on the schedule unit instead of the old type, so a legacy Mon/Wed/Fri "Weekly" quest (now Day-unit with a weekday filter) feeds `CompletedDailyQuests`. Only the Balanced Hero badge reads these three counters; the effect is that such quests now count toward its daily leg rather than its weekly one. Historical counter values are left untouched. |
| 7 | Grace window | **2 days, with a dedicated catch-up surface** (§5). Structurally this stops being a special case: a completion carries the date it counts for, so daily quests are backfillable like any other. |

### Still open

- **A weekly-streak badge.** Re-keying badges to the schedule unit leaves Week with no badge of its own. Worth
  adding, but it is a game-balance call rather than a technical one.
- **Whether `DurationMinutes` ships in phase 1** or waits for phase 3. It is one nullable column and nothing else
  reads it until the calendar export exists.
