# Finance module — reference

Personal-finance feature domain: income/expense tracking, hierarchical categories, budgets, analytics,
transaction corrections and monthly recurring templates. Built as an ordinary vertical slice under
`Application/Finance/`, keyed on **`UserProfile`** (not the auth `Account`), and following every convention in
[`ARCHITECTURE.md`](../ARCHITECTURE.md).

- **[`ARCHITECTURE.md` §13](../ARCHITECTURE.md#13-finance-module)** — the one-page overview.
- **This file** — the durable detail: model rules, locked decisions *and why they must not be undone*, API
  surface, migration/seeding hazards.
- **`docs/finance-api-schema.ts`** — the live FE contract (hand-written TS). **`docs/swagger.json`** — generated.
- Supersedes `FINANCE_MODULE_PLAN.md` (the phase-by-phase build checklist), deleted 2026-08-15. Recover it from
  git history if you ever need the build log; everything still true was folded into this file.

## Status (2026-08-15)

Core is **done and green** — 222 finance tests inside 460 total, `dotnet test Application.Tests` fully passing.
All slices, endpoints, background generation and migrations are in place; nothing in the module is half-built.

Deliberately **not** built (deprioritized by the owner, model leaves room): receipt attachments (would reuse
Cloudinary), multiple wallets + transfers, multi-currency (`Money` VO + FX), gamification hooks (XP/coins).
The module stays independent of Quests — the only cross-module dependency is `UserProfileId` for identity.

## Where the code lives

| Layer | Path |
|---|---|
| Entities | `Domain/Models/{FinanceTransaction,FinanceCategory,Budget,RecurringTransaction,MonthlyTotal}.cs` |
| Enums | `Domain/Enums/{FinanceTransactionTypeEnum,BudgetPeriodEnum}.cs` |
| Calculators (pure) | `Domain/Calculators/{FinancePeriodCalculator,BudgetProgressCalculator,OpeningBalanceCalculator,RecurrenceCalculator}.cs` |
| Currency allow-list | `Domain/ValueObjects/SupportedCurrencies.cs`, `Domain/ValueObjects/BudgetProgress.cs` |
| Repository contracts | `Domain/Interfaces/Repositories/I{FinanceTransaction,FinanceCategory,Budget,RecurringTransaction}Repository.cs` |
| EF configuration + seeding | `Infrastructure/Persistence/Configuration/{FinanceTransaction,FinanceCategory,Budget,RecurringTransaction}Configuration.cs` |
| Repositories | `Infrastructure/Persistence/Repositories/*` (via `UnitOfWork`) |
| Slices | `Application/Finance/{Categories,Transactions,Budgets,Analytics,RecurringTransactions,Settings}/` |
| Controllers | `Api/Controllers/{FinanceCategories,FinanceTransactions,Budgets,FinanceAnalytics,RecurringTransactions,FinanceSettings}Controller.cs` |
| Background generation | `Api/BackgroundTasks/GenerateRecurringTransactionsTask.cs` |
| Tests | `Application.Tests/Finance/{Models,Calculators,Categories,Transactions,Budgets,Analytics,RecurringTransactions,Settings}/` |

## Data model

**`FinanceTransaction`** — a single entity discriminated by `Type` (mirrors `Quest`), never a class hierarchy.
`Amount` is always positive; the sign is implied by the type. Beyond the obvious fields it carries
`IsPaid`, `RecurringTransactionId` (provenance), `CorrectsTransactionId` + `CorrectedAmount` (corrections), and
the computed `NetAmount = Amount - CorrectedAmount` — **the property every analytics query sums**.

**`FinanceCategory`** — self-referencing, exactly **two levels** (main → sub). `UserProfileId == null` marks a
seeded system row (like `Badge`). Sub-categories inherit `Type` *and* `IsSavings` from their parent.

**`Budget`** — overall (`CategoryId == null`) or per-category, `Monthly` or `Yearly`. Both may coexist for the
same period; uniqueness is `(UserProfileId, CategoryId, Period, Year, Month)`.

**`RecurringTransaction`** — a monthly template (rent, tuition, subscription). Not itself a transaction and
never appears in any aggregate. `LastMaterializedOn` is the generation watermark; `DayOfMonth` is 1–31.

**`MonthlyTotal`** — projection type for the grouped `(Year, Month, Type, Sum)` query behind `openingBalance`.

## Decisions (locked)

### Money & time

- **Money is `decimal(18,2)`, stored positive.** Direction comes from `Type`, never from a negative amount.
- **`OccurredOn` is a `DateOnly` (SQL `date`)** — a calendar fact, deliberately not a UTC instant, so it cannot
  drift across month boundaries when the user's timezone changes. Audit `CreatedAt`/`UpdatedAt` stay UTC.
  (`QuestOccurrence` periods were later migrated to the same reasoning — see ARCHITECTURE §6.)
  Backdating and future-dating are both allowed.
- **One currency per user**, ISO-4217 string on `UserProfile.Currency` (default `"USD"`, validated against
  `SupportedCurrencies`). Changing it does **not** convert historical amounts.
- **Hard delete for transactions.** Category delete is *blocked* while in use and accepts an **array of ids**
  (bulk, all-or-nothing in one unit of work): the whole request is rejected, naming the offenders, if any id is
  unowned, is a system category, has transactions, or is a main whose subs aren't in the same request.
- **Guiding principle: flexibility, not restriction.** Category is optional (uncategorized is legal); a
  transaction may tag a main *or* a sub; budgets are opt-in; custom categories are unrestricted.

### Categories and `IsSavings`

- **`IsSavings` is pure metadata.** Analytics totals, breakdowns and budget progress treat savings rows exactly
  like any other; netting them out is the client's job, and it can do that from `ExpenseByCategory` plus the
  category tree. Corrections (below) go the *other* way and are netted server-side — a deliberate asymmetry,
  because the FE cannot compute those: they need per-transaction parentage it only ever sees one page of.
- **Sub-categories strictly inherit `IsSavings` from their parent** — values sent for a sub are ignored, and
  setting it on a main cascades. Per-sub overrides were considered and **rejected**: a budget on a main rolls up
  all its subs, so subs that disagree about savings-ness make that total meaningless, and it would break the
  FE's one-line rule ("children of an `isSavings` main are savings"). *Subs disagreeing about `IsSavings` is the
  signal that they belong under different mains* — let the taxonomy carry it, not a per-row exception.
  Consequence: a sub under a *system* main can never be savings; the user creates their own main instead.
- **Money lent out is `IsSavings: false`**, even though you expect it back. Repayment is recorded as a
  *correction* against the original expense, which nets it to zero on its own; flagging it as savings would park
  an unrepaid loan under "set aside" indefinitely — precisely the case where it isn't. Same reasoning moved
  "Spłata długów" out of "Finanse i Oszczędności" into its own main, "Długi i Pożyczki".
- **A trip is a main category, not a sub of "Rozrywka i Inne"** (2026-08-09, after auditing two months of real
  transactions). A trip's cost spans bed, transport, food and tickets, so the number worth having is the total
  for the trip — which only a main can roll up. In the audited data one holiday was **22.6% of two months of
  spending** while being invisible, split across "Wyjścia ze znajomymi" and "Nieprzewidziane wydatki".
- **No generic "Inne" for expenses, deliberately.** The same audit found 98% of what had piled up in
  "Nieprzewidziane wydatki" had an obvious home that simply didn't exist yet (travel, furnishings, toiletries,
  electronics). A catch-all hides that signal: a growing "don't know" pile is supposed to mean *a category is
  missing*, and "Nieprzewidziane" should keep its literal meaning — a sudden, unplanned expense.

### Corrections (refunds, paybacks, reimbursements)

Money coming *back* against an earlier transaction. Logging it as separate "Other" income corrupts four numbers
at once: it inflates income-derived budgets, overstates the category, invents a fake income source, and breaks
month-over-month deltas when the payback lands in a later month.

- **A correction is a relation, not a type.** It is a `FinanceTransaction` pointing at its parent via
  `CorrectsTransactionId` and **inheriting the parent's `Type` and `CategoryId`** — the link carries the
  direction. The client sends neither.
  ⚠️ *Do not "fix" this to an Income-typed counter-entry:* `GetBudgetProgressQueryHandler` filters
  `Type = Expense`, so such a row would be silently dropped and budget progress would stay wrong while every
  other number got fixed. Inheriting also keeps `CreateTransaction`'s category/type consistency check valid
  unchanged, and makes `?categoryId=` return parent and corrections together.
- **The reserved `FinanceTransactionTypeEnum.Transfer` slot stays reserved** for the wallets backlog item. A
  correction must not consume it.
- **Netting is materialized on the parent** as `CorrectedAmount`, maintained by domain methods
  (`ApplyCorrection` / `RevertCorrection`); `NetAmount` is computed, not mapped. Chosen over read-time netting
  because a correction may sit in a different month than its parent, which would force all five analytics
  handlers into a second cross-period query plus type-filter special-casing. Accepted cost: derived state that
  must be kept in step on correction create/update/delete — all three atomic via `IUnitOfWork`.
- **Invariant `0 <= CorrectedAmount <= Amount`**, enforced in the entity. This is what guarantees no negative
  pie slices, budget bars or KPI tiles downstream. Over-correcting is a `Conflict`; "they paid back more than
  the bill" is a separate ordinary `Income` transaction, not a correction.
- **A correction nets against its parent's period and category, whatever its own `OccurredOn`.** There is no
  wallet or running balance in the module, so there's no cash ledger for an "the money actually arrived in
  August" reading to serve; every analytics endpoint is a period aggregate of economic cost. The correction's
  own date is kept for the record and shown on the row.
- **One level deep** — a correction cannot itself be corrected (mirrors the category tree rule).
- **Corrections are excluded from analytics entirely and from the top level of `GET /transactions`**, returned
  embedded in the parent DTO instead. One repository filter (`CorrectsTransactionId == null`) does this for all
  five analytics queries. It keeps paging correct — a correction dated in another month can never be orphaned
  onto a different page from its parent — and leaves History's row count unchanged.
- **`PUT /transactions/{id}` compares, then acts or rejects on an actual difference.** It is full-replacement
  and `Type`/`CategoryId` are non-optional, so the FE echoes the inherited values back on an ordinary edit; an
  unchanged echo must be a no-op, never an error, or editing a correction's note would be impossible.

  | Target | `Type` differs | `CategoryId` differs |
  |---|---|---|
  | is a correction | `Conflict` | `Conflict` |
  | has corrections | `Conflict` (`ChangeType` blocked) | allowed, **cascades** to its corrections |
  | ordinary | allowed | allowed |

  ⚠️ `FinanceTransactionTypeEnum` is a value type: an **omitted** `type` deserializes to `Income` (0), not null.
  On an Expense correction that reads as an attempted change and 409s — correct, loud behaviour, but it means
  the FE must **echo** `type`, never omit it. "Silently ignore" was rejected for exactly this reason: it would
  have masked that client bug.
- **`POST /{id}/corrections` returns the parent, not the correction.** Corrections are never rendered as
  standalone rows, so the client needs the parent back with its refreshed `NetAmount` and the correction
  embedded — that's the row it has to repaint.
- **Deleting**: a correction reverts its amount on the parent; deleting a parent removes its corrections first
  (the self-FK is `Restrict`). One `SaveChangesAsync` either way.

### `isPaid`

- **Pure metadata — an unpaid transaction is counted exactly as if it were already paid.** `totalExpense`,
  breakdowns, budget progress, trend and `openingBalance` all treat paid and unpaid alike. A logged-but-unpaid
  bill is money already spoken for; excluding it would make the dashboard optimistic on precisely the days the
  user needs it accurate. Same reading as `IsSavings`, and stated explicitly in the DTO docs — leaving it
  implicit is how the `IsSavings` question sat parked for two phases.
- **Defaults to `true`** on the ordinary create path regardless of `OccurredOn` (a date-dependent implicit
  default is surprising for API clients). **Exception: recurring materialization creates expenses unpaid** —
  those genuinely have not been paid, and that pairing gives the client an "upcoming bills" view for free.
- **Corrections are always `IsPaid = true`** — money that has come back has come back.

### `openingBalance`

- **Signed — no `max(0, …)` clamp.** A clamp hides what the user most needs (overspending would read "0 left
  over" rather than "−500") and, worse, **breaks the chain**: a month ending at −500 clamped to 0 erases that
  shortfall from every later month, so the running balance stops being a balance. Rendering negatives
  differently is a presentation choice the client already has patterns for.
- **Chain origin** is the user's earliest transaction month, whose `openingBalance` is 0.
- **No caching or denormalization** — the fold is over *months*, not transactions (one grouped SQL query, a few
  dozen rows at this scale). Note this is the opposite call from `CorrectedAmount`, for a different reason:
  there the denormalization bought uniform simplicity across five handlers, here it would buy only drift risk.
  Revisit past ~5 years of history, or if it shows up in profiling.
- The grouped query sums `Amount - CorrectedAmount` over `CorrectsTransactionId == null` rows as plain column
  expressions so it translates to SQL — **do not** load transaction rows and fold in memory.

### Recurring transactions

- **Auto-create via the existing `StartupTask` pattern** — no confirmation queue; a pending inbox that goes
  unserviced leaves the data just as wrong while adding a state machine.
- **Generation catches up to the latest state**, modelled on `ProcessOccurrencesTask` /
  `GenerateMissingOccurrences`: the repository pre-filters candidates in SQL (`GetForMaterializationAsync` —
  active templates whose watermark is null or before this month) so the handler never loads a template with
  nothing to do; the span is derived, not scheduled; there's an existence check before insert; one
  `SaveChangesAsync`; the task logs started / affected rows / finished inside try-catch. Consequence, same as
  the quest tasks: correct whenever it fires, harmless if it fires twice, so it needs no scheduler.
  ⚠️ `StartupTask` has no timer — generation is tied to process restarts. Not finance-specific: `ResetQuestsTask`
  and `ProcessOccurrencesTask` carry the same tradeoff. The catch-up design means promoting all of them to a
  `PeriodicTimer`-based `BackgroundService` later is contained and changes **no** API contract.
- **One deliberate deviation from the quest precedent: the `LastMaterializedOn` watermark.** Quests dedupe purely
  by checking whether a matching occurrence exists — right for a system-generated window nobody deletes, wrong
  for a **user-owned transaction they absolutely will delete**. Under pure existence-checking, deleting the
  August "Netflix 40" row means the next restart silently recreates it, which reads as a bug. The watermark
  distinguishes *never generated* from *generated and then deleted*. `RecurringTransactionId` stays too, but for
  a different job: provenance ("from a template" in the UI) plus a within-run existence check.
  **Do not "simplify" this to the quests' existence-check** — there is a regression test for exactly that.
- **Aggregate boundary — do not force the quest shape here.** `Quest` owns its occurrences, so it can construct
  them in place; `RecurringTransaction` does **not** own `FinanceTransaction`. So: `RecurrenceCalculator`
  (pure) returns the missing months, and the handler constructs rows via `FinanceTransaction.CreateRecurring(...)`
  and advances the watermark. Calculation in Domain, cross-aggregate construction in the handler.
- **A template never materializes the month it was created in** — `Create` stamps the watermark to the creation
  month. Contract line for the FE: *"a recurring template starts producing rows the month after it is created."*
  Rationale: the client's "Repeat monthly" tick posts the real transaction **and** the template in one submit,
  and that transaction carries no `RecurringTransactionId` for the dedupe to see, so materializing the creation
  month would duplicate it every time. The backend can't distinguish that path from a standalone create, so one
  rule covers both; a standalone template that should also cover the current month is served by the caller
  posting that first transaction itself.
- **Resuming a paused template re-arms the watermark to the current month** — a pause means "don't charge me for
  these months", not "charge me later", so it must not backfill the paused span.
- **Horizon: the current month only.** Never pre-create future months.
- **`dayOfMonth` past the end of a short month clamps to its last day** (31 → 28/29/30), matching the client's
  existing copy-to-month behaviour.
- **Deleting a template must not delete its materialized rows.** The FK stays `Restrict` in the database and the
  delete handler **nulls `RecurringTransactionId`** on the template's rows in the same unit of work — same
  posture as `ParentCategoryId` on categories. `Restrict` alone would make the delete fail outright for any
  template old enough to have materialized anything, i.e. every template worth deleting.
- **Materialized rows are ordinary transactions** — editable, deletable, correctable. Deleting one doesn't delete
  the template; deleting the template doesn't delete already-materialized rows.
- Use the injected **`IClock`**, not `SystemClock.Instance` (ARCHITECTURE §9 flags the static as the wrong habit;
  it also makes catch-up untestable without time travel).

### Analytics semantics

- **A budget on a main category rolls up its sub-categories' spending.** Fixing this was a bug fix, not a
  preference: `GetBudgetProgressQueryHandler` used to match `t.CategoryId == budget.CategoryId` exactly while the
  FE rolled up, so Dashboard and Statistics disagreed in production and any budget on a parent read as
  permanently unspent — which is most budgets, given 8 mains vs ~50 subs.
  **Documented consequence:** budgeting a parent *and* one of its subs counts that sub's spending toward both.
  That is correct nested-envelope behaviour, but it should be a written decision rather than a surprise.
- **Category breakdowns are *not* rolled up server-side** — deliberately. Per-assigned-category plus
  `ParentCategoryId` is right: the FE rolls up on the Dashboard and deliberately does not in Statistics.
  Rolling up server-side would remove a choice the client is actively using.
- Analytics queries aggregate **in memory** over `IFinanceTransactionRepository.GetForPeriodAsync`; only the
  `openingBalance` totals are a grouped SQL projection.

## API surface

All routes are `[Authorize]`, identity via `User.GetCurrentUserProfileId()`. Enums serialize as strings;
`DateOnly` is `"YYYY-MM-DD"`.

| Route | Verbs |
|---|---|
| `api/finance/categories` | `GET` (tree: system + own) · `POST` · `PUT /{id}` · `DELETE` (bulk, id array in body) |
| `api/finance/transactions` | `GET` (filters `from`/`to`/`type`/`categoryId` + paging) · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| `api/finance/transactions/{id}/corrections` | `POST` (returns the **parent**) |
| `api/finance/transactions/{id}/paid-status` | `PATCH` |
| `api/finance/budgets` | `GET` (by period) · `POST` · `PUT /{id}` · `DELETE /{id}` |
| `api/finance/recurring-transactions` | `GET` · `POST` · `PUT /{id}` (**partial** update) · `DELETE /{id}` |
| `api/finance/analytics` | `GET monthly-summary` · `yearly-summary` · `category-breakdown` · `budget-progress` · `spending-trend` |
| `api/finance/settings` | `GET` · `PUT currency` |

`PUT /recurring-transactions/{id}` taking a **partial** body diverges from `PUT /transactions` (full
replacement) and matches `UpdateBudgetRequest` — documented rather than fought, since it's the FE's contract.

Relational checks (ownership, category/type consistency, uniqueness) live in **handlers** → `NotFound` /
`Conflict`; validators cover **field** rules only. No `Program.cs` registration was needed for any of it except
`GenerateRecurringTransactionsTask` — handlers, validators and Mapster profiles are picked up by the existing
assembly scans, and all four repositories are exposed through `IUnitOfWork`.

## Persistence, migrations & seeding

Finance migrations, in order:

| Migration | What it does |
|---|---|
| `20260724163256_AddFinanceModule` | Tables + `UserProfile.Currency` |
| `20260724194734_UpdateFinanceSystemCategories` | Replaced placeholder seeds with the FE taxonomy |
| `20260726202201_AddTransactionCorrections` | `CorrectedAmount`, `CorrectsTransactionId` self-FK + index (additive) |
| `20260731193752_AddTransactionIsPaid` | `IsPaid bit NOT NULL DEFAULT 1` (existing rows backfill to paid) |
| `20260731202128_AddRecurringTransactions` | New table + `FinanceTransactions.RecurringTransactionId` FK + index |
| `20260801205008_PromoteDebtRepaymentToMainCategory`, `20260801211918_AddDebtAndLoansCategory` | "Długi i Pożyczki" (8) + moving "Spłata długów" under it |
| `20260809182143_AddTravelHygieneFurnishingElectronicsCategories` | "Podróże i Wakacje" (9) + subs, hygiene/furnishing/electronics; **reseeds the identity to 100 000** |

⚠️ **Seeded category ids share one IDENTITY sequence with user-created categories.** Discovered the hard way on
2026-08-09: a seed migration failed on `PK_FinanceCategories` because user rows had taken 150–153. `HasData`
writes explicit ids under `IDENTITY_INSERT`, which *raises* the counter but never reserves a range, so the
sequence eventually walks into whatever the next seed migration wants. Permanently fixed by reseeding the
identity to **100 000**: system categories live below it, user categories above, and inserting a lower explicit
id never drags the counter back down (SQL Server only ever raises it).

- Current id allocation: expense mains **1–6, 8, 9** (7 was a retired "Other", never reused), income mains
  **50–57** (flat, no subs), expense subs **101–148** and **158–166**.
- **Ids 149–157 are burned** and must never be seeded. Number new system categories from **167** up.
- The same hazard applies to **any** table mixing `HasData` rows with user-generated ones.
- The taxonomy is Polish display strings (no category i18n on the backend); icons are Ionicons outline names;
  subs carry no icon/color and inherit the parent's. Source of truth: `FinanceCategoryConfiguration` and
  `docs/categories.txt`.

**Environment notes** (see also the ARCHITECTURE testing section):

- `dotnet` on `PATH` is runtime-only — use the user-local SDK at `C:\Users\Swida\.dotnet\dotnet.exe`.
- EF design-time commands need an environment argument, and it must be **`-- production`**: `-- development`
  points at a local named instance that isn't running. There is no local DB; prod is migrated directly (a
  two-person app, deliberately).
  ```
  dotnet ef migrations list          --project Infrastructure --startup-project Api -- production
  dotnet ef database update          --project Infrastructure --startup-project Api -- production
  ```
- A new `IEntityTypeConfiguration` must actually be picked up by `AppDbContext` — an unregistered configuration
  fails silently and voids everything in it.
- Regenerating `docs/swagger.json`: run the Api with `ASPNETCORE_ENVIRONMENT=Development` and a chosen
  `ASPNETCORE_URLS`, then `GET /swagger/v1/swagger.json` (repo `docs/*.json` use CRLF). Note Development config
  points at **prod** and startup fires the hosted `StartupTask`s.

## Tests

`Application.Tests/Finance/` — 222 tests, split the way ARCHITECTURE §10 recommends:

- **Pure, no DB:** `Models/` (entity invariants) and `Calculators/` — `RecurrenceCalculator` month-length
  clamping, `OpeningBalanceCalculator` gaps/negatives/empty history, `BudgetProgressCalculator`,
  `FinancePeriodCalculator`.
- **Handlers:** CRUD + ownership per slice, bulk-delete all-or-nothing, correction lifecycle (create/update/
  delete keep `CorrectedAmount` in step), recurring **generation idempotency**, and the regression test that a
  **user-deleted occurrence is not resurrected** — the case the watermark exists for.
- **Analytics:** an expense in month M corrected in M+1 nets into M and leaves M+1 untouched; budget roll-up
  from a sub to its parent.
- **Validators get their own tests** — handlers are `new`-ed up directly, so `ValidationBehavior` never runs.

⚠️ The date-boundary logic (opening balance, recurrence) is exactly where the InMemory provider passes while real
SQL differs — especially the grouped `SUM`. That's why the logic sits in pure calculators; consider SQLite
in-memory if repository-level coverage is ever added.

## Front-end & contracts

Front-end work lives in the FE repo (`GoodieHabbi-FN`), not here. The backend contracts
`docs/finance-api-schema.ts` (the live, hand-written one) and `docs/swagger.json` are current as of 2026-08-01
and cover corrections, `isPaid`, `openingBalance` and the recurring endpoints.
