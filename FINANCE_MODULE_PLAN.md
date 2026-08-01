# Finance Module — Implementation Task List

> Working plan for the finance feature domain. This is the detailed, ordered build checklist;
> `ARCHITECTURE.md` stays the high-level base and will get a short pointer later, not this detail.
> Branch: `finance-module`.

## Status — Phases 1–9, 11 and 12 done (2026-07-31)

> **Phase 12 complete (12.0–12.3, 2026-07-31).** 208 finance tests pass, 415/416 overall (the 1 failure is the
> pre-existing inventory one below). **All migrations are applied** — verified with
> `dotnet ef migrations list ... -- production` returning no `(Pending)` rows. Note the environment argument:
> `-- development` points at a local named instance (`MCJPII\MSSQLSEVER`) that is not running, so DB commands
> need `-- production`. Two contract items remain: `docs/finance-api-schema.ts` is up to date and carries
> everything the FE needs, but `docs/swagger.json` still isn't regenerated, and 11.7/11.8 (FE views and the
> manual payback cleanup) are unchanged.

- **Built & green:** Phases 1–9 and 11 complete. Full solution builds clean; 136 finance tests pass
  (was 90; +46 for corrections), 329/330 overall — the 1 failure is the pre-existing inventory one below.
- **Migrations:**
  - `20260724163256_AddFinanceModule` — **applied to prod** (user has no local DB; deliberately migrated prod
    directly since it's a 2-person app).
  - `20260724194734_UpdateFinanceSystemCategories` — **applied** (confirmed by owner 2026-07-26). Replaced the
    placeholder seed set with the FE-supplied taxonomy from `docs/categories.txt` (6 expense mains + 47 subs +
    8 flat income = 61 rows). Ids: expense mains 1-6, income mains 50-57, expense subs 101-147.
  - `20260726202201_AddTransactionCorrections` — **applied** (confirmed 2026-07-31 against production).
    Additive only: `CorrectedAmount`
    `decimal(18,2) NOT NULL DEFAULT 0`, nullable `CorrectsTransactionId` int FK (self, `Restrict`), and its
    index. No backfill, no data loss.
  - `20260731193752_AddTransactionIsPaid` — **applied 2026-07-31.** Additive: `IsPaid bit NOT NULL DEFAULT 1`;
    existing rows backfill to paid via the column default.
  - `20260731202128_AddRecurringTransactions` — **applied 2026-07-31.** New `RecurringTransactions` table plus
    a nullable `FinanceTransactions.RecurringTransactionId` FK (`Restrict`) and its index. No backfill.
    Apply with `dotnet ef database update --project Infrastructure --startup-project Api -- production`
    (**`-- production`**, not `-- development` — see the status note above).
- **FE round 1 (applied 2026-07-24):** FE returned `docs/swagger-corrected.json`; the only requested change was
  `isSavings: boolean` on `FinanceCategoryDto` / `CreateFinanceCategoryRequest` / `UpdateFinanceCategoryRequest`.
  Implemented (see "Category `IsSavings`" below). `docs/swagger.json` regenerated from a live run and is now
  byte-identical to `swagger-corrected.json`; `docs/finance-api-schema.ts` updated with the semantics note.
  Everything else in the contract came back untouched — FE has implicitly signed off on it.
- **Phase 10 (deferred backlog) — deprioritized by the owner 2026-07-31.** Receipt attachments,
  wallets/transfers, multi-currency and gamification are all "nice to have later", none scheduled. Finance is
  being built as a **standalone module**; room is left for a future `UserProfile` integration but nothing is
  built toward it.
- **Phase 12 (paid status, opening balance, recurring transactions) specced 2026-07-31, not started.** Driven
  by the FE handoff at `GoodieHabbi-FN/docs/finance-backend-todo.md` — the FE has already shipped all three
  client-side, each degrading silently until the backend catches up, so the items ship independently in any
  order. Also carries **12.0, a verified live bug** (parent-category budgets ignore sub-category spending) that
  makes Dashboard and Statistics disagree today.
- **Phase 11 (transaction corrections) — BACKEND DONE 2026-07-26.** 11.1–11.6 built and green; `Program.cs`
  needed no changes, as predicted. Two decisions settled during the build, both recorded in the Phase 11 section:
  editing a correction accepts an echoed-back `type`/`categoryId` and only 409s on an actual change; and
  `POST /{id}/corrections` returns the **parent**, not the correction. **Still outstanding:** the migration is
  unapplied (above), 11.7's FE work lives in the front-end repo, `docs/swagger.json` needs regenerating (see
  note below), and 11.8 is the owner's manual data cleanup.
- **`docs/swagger.json` is not in this repo** — it and `swagger-corrected.json` were never committed and are no
  longer on disk, so the round-1 artifacts are gone. Regenerate the usual way: run the Api with
  `ASPNETCORE_ENVIRONMENT=Development` + a chosen `ASPNETCORE_URLS`, then `GET /swagger/v1/swagger.json`
  (repo `docs/*.json` use CRLF). Left to the owner because Development config points at **prod** and startup
  also fires the four hosted `StartupTask`s. `docs/finance-api-schema.ts` is up to date and is the live FE contract.
- **Open questions — all now resolved:**
  1. ~~Should a **main-category budget** roll up its sub-categories' spending?~~ — **YES, and it turned out to
     be a bug, not a preference.** The FE already rolls up, so the two screens disagree in production today.
     Scheduled as Phase 12.0.
  2. ~~Should **category breakdowns** be rolled up to mains server-side?~~ — **No, leave as-is.**
     Per-assigned-category plus `ParentCategoryId` is correct: the FE rolls up on the Dashboard and
     deliberately does not in Statistics. Server-side roll-up would remove a choice the client is using.
  3. ~~`isSavings` analytics impact~~ — **ANSWERED 2026-07-26: stays pure metadata.** The FE nets savings
     client-side and that stays its job, because `ExpenseByCategory` + the category tree gives it enough to do so.
     Corrections (Phase 11) go the *other* way and are netted server-side, because the FE cannot compute those —
     they need per-transaction parentage it only ever sees one page of. Deliberate asymmetry, not an oversight.
- **Known pre-existing failure (not ours):** `GetUserInventoryItemsQueryHandlerTests` — `TestBase` doesn't mock
  `IUrlBuilder.BuildShopItemThumbnailUrl`, so `ItemUrl` is null. One-line fix in shared test infra if desired.
- **Env note:** `dotnet` is not on PATH here → use `C:\Users\Swida\.dotnet\dotnet.exe`; EF design-time commands
  need the `-- development` argument to load `appsettings.Development.json`.

## Design decisions (locked)

- **Scope v1:** income/expense transactions · hierarchical categories (main → sub, seeded + custom) ·
  budgets & period planning · analytics/summary endpoints.
- **Currency:** single per user, ISO-4217 string on `UserProfile.Currency` (default `"USD"`).
  Changing it does **not** convert historical amounts.
- **Money:** `decimal(18,2)`. Amounts stored positive; sign implied by transaction `Type`.
- **Transaction date:** `OccurredOn : DateOnly` (SQL `date`) — a calendar date, no time, no timezone.
  Audit fields (`CreatedAt`/`UpdatedAt`) stay **UTC** per existing convention. Backdating is allowed.
- **Delete policy:** hard delete for transactions. Category delete is **blocked** if in use;
  category delete endpoint accepts an **array of ids** (bulk), all-or-nothing per unit of work.
- **Budgets:** overall (category = null) and per-category budgets may coexist for the same period.
- **Category `IsSavings`** (added 2026-07-24 at FE request): a descriptive flag meaning "money set aside rather
  than consumed". Deliberately **pure metadata** — analytics totals, breakdowns and budget progress all treat
  savings transactions exactly like any other; presentation is the client's call. Sub-categories **inherit** it
  from their parent (same rule as `Type`): setting it on a main cascades to its subs, sending it for a sub is
  ignored. Consequence: a sub under a *system* main can never be savings — the user creates their own main
  instead. No seeded system category is flagged as savings, and the flag is allowed on Income and Expense alike.
  Chosen over an independently-settable flag because it keeps trees coherent, which is what a future
  analytics-aware reading (exclude savings from spend / report `totalSaved`) would need anyway.
- **Transaction corrections** (specced 2026-07-26, Phase 11): money coming back against an earlier transaction
  (refund, payback, reimbursement) is a `FinanceTransaction` linked to its parent via `CorrectsTransactionId`,
  **inheriting the parent's `Type` and `CategoryId`** — the link carries the direction, not the type. Netting is
  materialized on the parent as `CorrectedAmount` (`NetAmount = Amount - CorrectedAmount`), invariant
  `0 <= CorrectedAmount <= Amount`, and a correction nets against its *parent's* period regardless of its own
  `OccurredOn`. Corrections are excluded from analytics and from the top level of `GET /transactions`
  (embedded in the parent instead). Full rationale in Phase 11.
- **Guiding principle — flexibility, not restriction:** category is optional (uncategorized allowed);
  a transaction may tag either a main or a sub category; budgets are opt-in; custom categories are unrestricted.
- **Gamification:** none for now (no XP/coins coupling); model leaves room to add later.
- **Deferred (designed-for, not built):** recurring transactions, multiple wallets + transfers,
  multi-currency (`Money` VO + FX), receipt attachments (reuse Cloudinary), gamification hooks.

---

## Phase 1 — Domain (`Domain/`) — ✅ DONE (builds clean)

- [ ] `Enums/FinanceTransactionTypeEnum.cs` — `{ Income = 0, Expense = 1 }` (leave room for `Transfer`).
- [ ] `Enums/BudgetPeriodEnum.cs` — `{ Monthly = 0, Yearly = 1 }`.
- [ ] `Models/UserProfile.cs` — add `string Currency { get; private set; } = "USD"`,
      `UpdateCurrency(string code)` (validate against an allow-list, throw `InvalidArgumentException`),
      `ICollection<FinanceTransaction> FinanceTransactions`, `ICollection<FinanceCategory> FinanceCategories`,
      `ICollection<Budget> Budgets`; clear them in `WipeoutData()`.
- [ ] `Models/FinanceCategory.cs : EntityBase` — self-referencing, 2 levels max.
      - Props: `Id`, `UserProfileId : int?` (null = system/seeded), `ParentCategoryId : int?` (null = main),
        `Name`, `Type : FinanceTransactionTypeEnum`, `Color?`, `Icon?`, `IsSystem : bool`,
        nav `ParentCategory` / `SubCategories`, `Transactions`.
      - Factories: `CreateMain(userProfileId, name, type, color, icon)`,
        `CreateSub(parent, name, color, icon)`, `CreateSystem(...)` (for seeding).
      - Invariants: parent must be a main category (depth ≤ 2); `sub.Type == parent.Type`;
        can't nest under a foreign/mismatched category. Behavior: `Rename`, `UpdateColor`, `UpdateIcon`.
- [ ] `Models/FinanceTransaction.cs : EntityBase` — single entity discriminated by `Type` (mirror `Quest`).
      - Props: `Id`, `UserProfileId`, `Type`, `Amount : decimal`, `CategoryId : int?`,
        `OccurredOn : DateOnly`, `Note?`.
      - Factory `Create(...)`; methods `UpdateAmount` (`> 0`), `Recategorize`, `UpdateDate`, `UpdateNote`.
- [ ] `Models/Budget.cs : EntityBase`.
      - Props: `Id`, `UserProfileId`, `CategoryId : int?` (null = overall), `Period : BudgetPeriodEnum`,
        `Year : int`, `Month : int?` (1..12, null when yearly), `LimitAmount : decimal`.
      - Factory `Create(...)`; method `UpdateLimit` (`> 0`).
- [ ] `Calculators/FinancePeriodCalculator.cs` — `(Year, Month?)` → `DateOnly` start/end bounds (timezone-free).
- [ ] `Calculators/BudgetProgressCalculator.cs` — `spent` vs `limit` → remaining, percent, over/under (pure).
- [ ] `Interfaces/Repositories/IFinanceCategoryRepository.cs`, `IFinanceTransactionRepository.cs`,
      `IBudgetRepository.cs` (each `: IBaseRepository<T>`). Query methods:
      - Category: `GetUserCategoryTreeAsync(userProfileId)` (system + own), `GetOwnedByIdAsync`,
        `IsNameUniqueUnderParentAsync`, `AnyTransactionsForCategoriesAsync(ids)`, `GetOwnedByIdsAsync(ids)`.
      - Transaction: `GetOwnedByIdAsync`, `GetUserTransactionsAsync(filter: dateRange/type/categoryId, paging)`,
        `GetForPeriodAsync`, `SumByCategoryForPeriodAsync` (SQL GroupBy projection),
        `SumByTypeForPeriodAsync`, `MonthlyTotalsForYearAsync`.
      - Budget: `GetOwnedByIdAsync`, `GetUserBudgetsAsync(year, month?)`, `ExistsForCategoryPeriodAsync`.

## Phase 2 — Persistence (`Infrastructure/`) + migration — ✅ DONE (builds clean; migration 20260724163256_AddFinanceModule)

- [ ] `Persistence/Configuration/FinanceCategoryConfiguration.cs` — self-FK `ParentCategoryId`
      (`DeleteBehavior.Restrict`), nullable `UserProfileId` FK (`NoAction`, manual delete on account wipe),
      index `(UserProfileId, ParentCategoryId)`, `Name` max length.
- [ ] `Persistence/Configuration/FinanceTransactionConfiguration.cs` — `Amount` `HasPrecision(18,2)`,
      `OccurredOn` as `date`, indexes `(UserProfileId, OccurredOn)` and `(UserProfileId, CategoryId, OccurredOn)`,
      category FK `NoAction` (delete blocked in app layer).
- [ ] `Persistence/Configuration/BudgetConfiguration.cs` — `LimitAmount` `HasPrecision(18,2)`,
      unique index `(UserProfileId, CategoryId, Period, Year, Month)`.
- [ ] `AppDbContext.cs` — add `DbSet`s; **seed system categories** in `OnModelCreating` via `HasData`
      (mirror the Badge seeding). Example tree — Expense: *Home → {Rent, Internet, Electricity, Water}*,
      *Food → {Groceries, Restaurants}*, *Transport → {Fuel, Public transport}*, *Health*, *Entertainment*;
      Income: *Salary, Bonus, Gifts, Investments*. ⚠️ `HasData` behaves differently under the InMemory
      test provider (ARCHITECTURE §10) — account for it in tests.
- [ ] `Persistence/Repositories/{FinanceCategoryRepository,FinanceTransactionRepository,BudgetRepository}.cs`
      (`: BaseRepository<T>`).
- [ ] `Domain/Interfaces/IUnitOfWork.cs` + `Persistence/UnitOfWork.cs` — add the three lazy repositories.
- [ ] EF migration: `dotnet ef migrations add AddFinanceModule` (includes `UserProfile.Currency`). Review SQL.

## Phase 3 — Application: Categories (`Application/Finance/Categories/`) — ✅ DONE (builds clean)

- [ ] `Commands/CreateFinanceCategory/` — command/handler/validator/request/response/mapping.
      Handles both main (no parent) and sub (with `ParentCategoryId`); validate ownership + type consistency
      + name uniqueness under parent.
- [ ] `Commands/UpdateFinanceCategory/` — rename/color/icon; block editing `IsSystem` categories.
- [ ] `Commands/DeleteFinanceCategories/` — **bulk**: `record DeleteFinanceCategoriesCommand(IReadOnlyList<int> CategoryIds, int UserProfileId)`.
      All-or-nothing: reject the whole request (naming offenders) if any id is not owned, is a system category,
      has transactions, or is a main with sub-categories not included in the same request. One `SaveChangesAsync`.
- [ ] `Queries/GetUserCategoryTree/` — returns mains with nested subs (system + user's own).
- [ ] `Dtos/FinanceCategoryDto.cs` (recursive `Children`), mapping profiles.

## Phase 4 — Application: Transactions (`Application/Finance/Transactions/`) — ✅ DONE (builds clean)

- [ ] `Commands/CreateTransaction/` — `Amount > 0`; `CategoryId` optional; if set, must be owned/system and
      `Type`-consistent; `OccurredOn` any date (past/future allowed).
- [ ] `Commands/UpdateTransaction/` — amount/category/date/note; ownership check.
- [ ] `Commands/DeleteTransaction/` — hard delete; ownership check.
- [ ] `Queries/GetTransactions/` — filter (date range, type, categoryId) + paging; `Queries/GetTransactionById/`.
- [ ] `Dtos/TransactionDto.cs` + mapping.

## Phase 5 — Application: Budgets (`Application/Finance/Budgets/`) — ✅ DONE (builds clean)

- [ ] `Commands/CreateBudget/` — overall (null category) or per-category; unique per period; `LimitAmount > 0`.
- [ ] `Commands/UpdateBudget/`, `Commands/DeleteBudget/`.
- [ ] `Queries/GetBudgets/` — for a `(Year, Month?)` period.
- [ ] `Dtos/BudgetDto.cs` + mapping.

## Phase 6 — Application: Analytics (`Application/Finance/Analytics/Queries/`) — ✅ DONE (builds clean)

> FE handoff: TypeScript schema exported to `docs/finance-api-schema.ts` (enums, DTOs, request/query shapes, planned routes).
> Two items await FE input: (a) whether a main-category budget should roll up its sub-categories' spending
> (currently exact category match); (b) whether category breakdowns should be rolled up to mains server-side
> (currently per-assigned-category, with `ParentCategoryId` so the client can roll up).


- [ ] `GetMonthlySummary/` — income total, expense total, net + per-category breakdown.
- [ ] `GetYearlySummary/` — per-month totals + per-category.
- [ ] `GetCategoryBreakdown/` — pie-chart-ready for a period.
- [ ] `GetBudgetProgress/` — actual vs planned per category (uses `BudgetProgressCalculator`).
- [ ] `GetSpendingTrend/` — line-chart points across months.
- [ ] `Dtos/` — `MonthlySummaryDto`, `CategoryBreakdownItemDto`, `BudgetProgressDto`, `SpendingTrendPointDto`.

## Phase 7 — Application: Settings — ✅ DONE (builds clean; + GetFinanceSettings read)

- [ ] `Application/Finance/Settings/Commands/UpdateCurrency/` — validate against ISO-4217 allow-list.

## Phase 8 — Api (`Api/Controllers/`) — ✅ DONE (builds clean; no Program.cs changes needed)

- [ ] `FinanceCategoriesController` → `api/finance/categories` (incl. bulk delete accepting an id array in body).
- [ ] `FinanceTransactionsController` → `api/finance/transactions`.
- [ ] `BudgetsController` → `api/finance/budgets`.
- [ ] `FinanceAnalyticsController` → `api/finance/analytics`.
- [ ] Each thin, `[Authorize]`, `ISender` (+ `IMapper`), identity via `JwtHelpers.GetCurrentUserProfileId(User)`.
- [ ] **DI check:** handlers/validators/Mapster profiles are auto-scanned and repositories live inside
      `UnitOfWork`, so `Program.cs` should need **no** changes. Confirm nothing new needs registration.

## Phase 9 — Tests (`Application.Tests`) — ✅ DONE (90 finance tests pass; 0 regressions)

> Pre-existing unrelated failure: `GetUserInventoryItemsQueryHandlerTests.Handle_ShouldReturnCorrectItems_WhenUsingSeedData`
> — `TestBase` doesn't mock `IUrlBuilder.BuildShopItemThumbnailUrl`, so `ItemUrl` is null. Not touched by finance work.


- [ ] Domain unit tests (no DB): `FinanceCategory` invariants (depth, type match, foreign-parent),
      `FinanceTransaction` (`Amount > 0`), `Budget`, `FinancePeriodCalculator`, `BudgetProgressCalculator`.
- [ ] Handler tests using builders/factories (not reflection); cover bulk-delete all-or-nothing behavior.
- [ ] Dedicated validator tests (ValidationBehavior isn't exercised when handlers are invoked directly).

## Phase 10 — Backlog (deferred, model already leaves room)

- [ ] Recurring transactions: `RecurringTransaction` + `RecurrenceIntervalEnum` +
      `GenerateRecurringTransactionsTask : StartupTask` (MediatR command, like `ResetQuestsTask`).
- [ ] Multiple wallets + transfers: `Wallet` entity + `FinanceTransactionTypeEnum.Transfer` + running balances.
- [ ] Multi-currency: promote `Amount` to a `Money` value object + FX rates.
- [ ] Receipt attachments via the existing Cloudinary photo service.
- [ ] Optional gamification hooks (XP/coins) on logging / staying under budget.

---

## Phase 11 — Transaction corrections (refunds / paybacks / reimbursements) — ✅ BACKEND DONE (11.1–11.6; builds clean, 136 finance tests pass)

### Problem

Money that comes *back* against an earlier transaction has no home in the model, so it gets logged as a
separate `Income` / "Other" row. That is not just untidy — it corrupts four numbers. Example: dinner paid for
friends, 400, of which 300 comes back.

| Broken today | Why |
|---|---|
| Monthly budget inflated by 300 | FE derives `budget = totalIncome`; a payback was never income |
| Category overstated by 300 | `ExpenseByCategory` shows 400 for the food category → false over-budget |
| Fake income source | `IncomeByCategory` accumulates paybacks under "Other" |
| Month-over-month deltas wrong | Payback in the next month makes both months wrong (annual net right by luck) |

The same shape covers: product returns · partial refunds · double charges/chargebacks · deposits returned ·
**employer-reimbursed work expenses** · insurance payouts · cashback arriving later · and the mirror direction
(refunding a freelance client, returning overpaid salary — a *faktura korygująca*). Hence "correction", not "refund".

### Decisions (locked)

- **A correction is a relation, not a type.** A correction is a `FinanceTransaction` pointing at another via
  `CorrectsTransactionId`. It **inherits `Type` and `CategoryId` from its parent** — the client sends neither.
  Direction (money flowing the other way) is carried by the link.
  ⚠️ *Rationale, do not undo:* `GetBudgetProgressQueryHandler` calls `GetForPeriodAsync(..., Type = Expense, ...)`.
  An `Income`-typed counter-entry would be silently dropped by that filter, leaving budget progress wrong while
  every other number got fixed. Inheriting the parent's type also keeps `CreateTransaction`'s existing
  category/type consistency check valid unchanged, and makes `?categoryId=` return parent + corrections together.
- **The reserved `FinanceTransactionTypeEnum.Transfer` slot stays reserved** for the wallets backlog item.
  A correction must not consume it.
- **Netting is materialized on the parent** as `CorrectedAmount`, maintained by domain methods.
  `NetAmount => Amount - CorrectedAmount` is computed, not mapped.
  *Chosen over read-time netting* because a correction may sit in a different month than its parent, so
  read-time netting would force every one of the five analytics handlers into a second cross-period query plus
  type-filter special-casing. With the rollup, the analytics change is `Amount` → `NetAmount` and nothing else.
  *Accepted cost:* derived state in the DB, so it must be maintained on correction create/update/delete —
  three handlers, all atomic via the existing `IUnitOfWork`.
- **Invariant `0 <= CorrectedAmount <= Amount`**, enforced in the entity. This is what guarantees no negative
  pie slices, budget bars or KPI tiles downstream. Over-correcting is a `Conflict`; "they paid back more than
  the bill" is a separate ordinary `Income` transaction, not a correction.
- **Attribution: a correction nets against its parent's period and category, regardless of its own `OccurredOn`.**
  *Rationale:* the module has no wallet or running balance (deferred, Phase 10), so there is no cash ledger for a
  "the money actually arrived in August" reading to serve; every analytics endpoint is a period aggregate of
  economic cost. The correction's own date is kept for the record and shown on the FE row.
- **One level deep** — a correction cannot itself be corrected (mirrors the category tree rule).
- **Corrections are excluded from analytics entirely, and from the top level of `GET /transactions`**; they are
  returned embedded in the parent DTO. This keeps FE paging correct (a correction dated in another month can
  never be orphaned onto a different page from its parent) and leaves History's row count unchanged.
- **Editing a correction accepts the inherited values echoed back** (settled 2026-07-26 during the build).
  `PUT /transactions/{id}` is a full-replacement command — `Type` and `CategoryId` are non-optional — so the FE
  round-trips the whole object. The handler therefore 409s only when a submitted value *differs* from the
  correction's current one; an unchanged echo is accepted silently. Without this, editing a correction's note
  would be impossible without a special client payload or a third endpoint. Same rule for a parent's `ChangeType`.
- **`POST /{id}/corrections` returns the parent, not the correction** (settled 2026-07-26). Corrections are never
  rendered as standalone rows, so the client needs the parent back with its refreshed `NetAmount` and the new
  correction embedded — that is the row it has to repaint.
- **`IsSavings` stays pure metadata — this answers parked open question #3.** Savings netting remains the
  client's job; corrections must be server-side. The distinction is *not* inconsistency: the FE can compute
  savings because `ExpenseByCategory` plus the category tree is sufficient information, but it cannot compute
  corrections, which need per-transaction parentage the FE only ever sees one page of. Recorded here so the next
  analytics change doesn't conflate the two.

### 11.1 — Domain (`Domain/`) — ✅ DONE

- [ ] `Models/FinanceTransaction.cs` — add `int? CorrectsTransactionId { get; private set; }`,
      `decimal CorrectedAmount { get; private set; }`, navs `CorrectsTransaction` / `Corrections`,
      computed `NetAmount => Amount - CorrectedAmount`, `IsCorrection => CorrectsTransactionId is not null`.
- [ ] New factory `CreateCorrection(userProfileId, parent, amount, occurredOn, note)` — copies `Type` and
      `CategoryId` off the parent, sets the FK. Leave the existing `Create` untouched.
- [ ] `ApplyCorrection(decimal)` / `RevertCorrection(decimal)` on the parent, enforcing the invariant
      (`InvalidArgumentException` as the backstop; handlers raise `ConflictException` for the user-facing path).
- [ ] Guard `UpdateAmount` — a parent's amount may not drop below its `CorrectedAmount`.
- [ ] Guard `ChangeType` — blocked while corrections exist (flipping Income↔Expense under them is nonsense).
      `Recategorize` is allowed and **cascades** to corrections (blocking it would mean "you can't recategorize a
      dinner you got a refund for").

### 11.2 — Persistence (`Infrastructure/`) + migration — ✅ DONE (migration 20260726202201, unapplied)

- [ ] `Configuration/FinanceTransactionConfiguration.cs` — self-FK via
      `HasMany(t => t.Corrections).WithOne(t => t.CorrectsTransaction).HasForeignKey(t => t.CorrectsTransactionId)`
      with `OnDelete(DeleteBehavior.Restrict)` (same posture as `ParentCategoryId` on categories — the app layer
      cascades and returns a friendly error); `CorrectedAmount` `HasPrecision(18,2)`; index on
      `CorrectsTransactionId`; `builder.Ignore(t => t.NetAmount)`.
- [ ] `FinanceTransactionRepository.GetForPeriodAsync` — add `.Where(t => t.CorrectsTransactionId == null)`.
      This one line is what keeps corrections out of all five analytics queries.
- [ ] `FinanceTransactionRepository.GetUserTransactionsAsync` — same filter on the base query (so `TotalCount`
      counts parents only — intended), then a second query loading corrections for the returned parent ids.
- [ ] `IFinanceTransactionRepository` — add `GetCorrectionsForParentsAsync(parentIds, ...)` and
      `GetOwnedWithCorrectionsAsync(id, userProfileId, ...)` (a separate method, so existing call sites are untouched).
- [ ] `AnyForCategoriesAsync` needs **no** change — corrections carry the parent's category, so the
      category-delete guard keeps working.
- [ ] Migration `AddTransactionCorrections` — additive only: nullable `int` FK + `decimal(18,2) NOT NULL DEFAULT 0`
      + index. No backfill.
      ⚠️ `20260724194734_UpdateFinanceSystemCategories` is **still unapplied**; a `database update` will now apply
      both in order. Apply and verify that one first.

### 11.3 — Application (`Application/Finance/Transactions/`) — ✅ DONE

- [ ] New slice `Commands/AddCorrection/` — `AddCorrectionRequest(Amount, OccurredOn, Note)`,
      `AddCorrectionCommand(TransactionId, Amount, OccurredOn, Note, UserProfileId) : ICommand<TransactionDto>`,
      handler, validator, Mapster profile. A dedicated command rather than overloading `CreateTransaction` with
      fields that would have to be conditionally ignored.
  - Handler: load parent (tracked) → `NotFoundException`; `parent.IsCorrection` → `ConflictException`;
    over-correction → `ConflictException`; `parent.ApplyCorrection(amount)` + `CreateCorrection(...)` +
    `AddAsync`; **one** `SaveChangesAsync`.
  - Validator: `Amount > 0`, note ≤ 250. Relational checks stay in the handler, per module convention.
- [ ] `Commands/UpdateTransaction/` — `PUT` is full-replacement and `Type`/`CategoryId` are non-optional on the
      command, so the FE echoes the inherited values back on an ordinary edit. **Rule: compare, then act or
      reject on actual difference** — an echoed value is a no-op, never an error. Uniform across all three cases:

      | Target | `Type` differs | `CategoryId` differs |
      |---|---|---|
      | is a correction | `Conflict` | `Conflict` |
      | has corrections | `Conflict` (`ChangeType` blocked) | allowed, **cascades** to its corrections |
      | ordinary | allowed | allowed |

  - Compare against the target's **own** stored values (equal to the parent's by construction, via the cascade) —
    no second load. `int?` equality covers the uncategorized (null == null) case.
  - Correction path: amount/date/note only; recompute the parent via `RevertCorrection(old)` then
    `ApplyCorrection(new)`. Parent path: `UpdateAmount` guarded by the invariant.
  - 409 message must name the fix, since it lives on another resource: *"A correction inherits its type and
    category from the transaction it corrects — recategorize the parent instead."*
  - ⚠️ `FinanceTransactionTypeEnum` is a value type: an **omitted** `type` deserializes to `Income` (0), not null.
    On an Expense correction that reads as an attempted change and 409s — correct (loud) behaviour, but it means
    the FE must **echo** `type`, never omit it. Rejected "silently ignore" for exactly this reason: it would have
    masked that client bug.
  - Relational check → handler → `Conflict`, per module convention. Nothing for the validator.
- [ ] `Commands/DeleteTransaction/` — correction → `parent.RevertCorrection(amount)` then remove; parent →
      remove its corrections first (FK is `Restrict`). One `SaveChangesAsync` either way.
- [ ] `Dtos/TransactionDto.cs` — `+ CorrectsTransactionId`, `+ CorrectedAmount`, `+ NetAmount`,
      `+ Corrections: List<TransactionDto>`. Mapster picks `NetAmount` up from the computed property.
- [ ] `Queries/GetTransactions/` + `Queries/GetTransactionById/` — map the embedded corrections.

### 11.4 — Analytics: `Amount` → `NetAmount` (12 sites, 6 files) — ✅ DONE (count was exact)

- [ ] `Analytics/Common/FinanceAnalyticsHelper.cs` (2 — the group total and the per-category sum)
- [ ] `Queries/GetMonthlySummary/GetMonthlySummaryQueryHandler.cs` (2)
- [ ] `Queries/GetYearlySummary/GetYearlySummaryQueryHandler.cs` (4)
- [ ] `Queries/GetCategoryBreakdown/GetCategoryBreakdownQueryHandler.cs` (1)
- [ ] `Queries/GetSpendingTrend/GetSpendingTrendQueryHandler.cs` (2)
- [ ] `Queries/GetBudgetProgress/GetBudgetProgressQueryHandler.cs` (1)

### 11.5 — Api — ✅ DONE (no `Program.cs` changes needed, as predicted)

- [ ] `FinanceTransactionsController` — `POST /api/finance/transactions/{id}/corrections`.
      No `Program.cs` changes expected (handler/validator/Mapster assembly scans, repo lives in `UnitOfWork`).

### 11.6 — Tests (`Application.Tests`) — ✅ DONE (+46 tests: 16 domain, 7 add-correction, 12 lifecycle, 4 analytics, 7 validator)

- [ ] Domain, no DB: `ApplyCorrection`/`RevertCorrection` invariant, over-correction throws, `NetAmount`,
      `UpdateAmount` below `CorrectedAmount` throws, `CreateCorrection` inherits type + category.
- [ ] Handlers: add/update/delete keep `CorrectedAmount` in step; deleting a parent removes its corrections;
      correction-of-a-correction → `Conflict`.
- [ ] Analytics: expense in month M with a correction dated M+1 → M is net, M+1 unaffected, and
      `GetBudgetProgress` (the `Type = Expense` filter path) nets correctly.
- [ ] Dedicated `AddCorrectionCommandValidator` tests (`ValidationBehavior` isn't exercised by handler tests).

### 11.7 — FE contract + views — 🟡 schema done, FE repo + swagger outstanding

- [x] `docs/finance-api-schema.ts` — 4 new `TransactionDto` fields + `AddCorrectionRequest` + the new route,
      plus a "TRANSACTION CORRECTIONS" block spelling out the client rules.
- [ ] `docs/swagger.json` — **not regenerated.** The file was never committed and is gone from disk; producing
      one means running the API (Development config → prod DB, plus the four hosted `StartupTask`s). Owner's call.
- [ ] FE: `finance.contract.ts` (fields), `finance-api.ts` (`useAddCorrectionMutation`, invalidating
      `financeTransactions` + `financeAnalytics`), `add-transaction-modal.tsx` (correction mode — hides the type
      toggle and both category grids, leaving amount/date/note), `history.tsx` (render `netAmount` + a `↩` badge,
      a `↩` action button beside the existing trash icon, and **switch the client-side header totals off
      `tx.amount`** or they will disagree with every other screen).
- [ ] Dashboard and Statistics need **no** FE changes — they read server aggregates, which arrive already net.

### 11.8 — Data cleanup (manual, owner-confirmed)

- [ ] Existing prod rows where an `Income`/"Other" entry is really a payback are **not** converted by the
      migration. Owner will fix by hand: delete the payback income row, then add a correction on the original
      expense. Confirmed acceptable — the affected data is the owner's own.

---

## Phase 12 — Paid status, opening balance, recurring transactions — 📋 SPECCED, not started

Driven by the FE handoff (`GoodieHabbi-FN/docs/finance-backend-todo.md`). **The FE has already shipped the
client side of all three** — contracts and RTK Query endpoints are written as if the backend supports them, and
each degrades silently until it does (`isPaid` undefined reads as paid, missing `openingBalance` hides the card,
the recurring endpoints fail into a handled empty state). So none of this is a breaking rollout, and the three
items can ship independently in any order.

### Decisions (locked)

- **`isPaid` is pure metadata — no analytics impact. An unpaid transaction is counted exactly as if it were
  already paid** (owner's wording: *"it is a planned payment, but let's consider it as paid already"*).
  `totalExpense`, breakdowns, budget progress, trend and `openingBalance` all treat paid and unpaid alike.
  Same reading as `IsSavings`, and for the same reason: presentation is the client's call. Substantively it is
  also the right default — a logged-but-unpaid bill is money already spoken for, and excluding it would make
  the dashboard optimistic on precisely the days the user needs it accurate. This mirrors the
  *committed vs. consumed* distinction the FE settled on for savings.
  ⚠️ State this explicitly in the DTO docs; leaving it implicit is how the `IsSavings` question sat parked for
  two phases.
- **Default `isPaid`** is `true` on the ordinary create path regardless of `OccurredOn` — answering the FE's
  parked question. A date-dependent implicit default is surprising for API clients, and the FE always sends an
  explicit value anyway. **The exception is recurring materialization**, which creates expenses with
  `isPaid = false` (see below) — those genuinely have not been paid, and that pairing is what gives the FE an
  "upcoming bills" view for free.
- **`openingBalance` is signed — no `max(0, …)` clamp.** Rejecting the FE's proposed clamp on two grounds.
  First, it hides the information the user most needs: overspending would read as "0 left over" rather than
  "−500". Second and more seriously, **clamping breaks the chain** — a month ending at −500 clamped to 0 erases
  that shortfall from every later month, so the running balance stops being a balance. Return the true value;
  if the FE wants to render negatives differently that is a presentation choice, and it already has the
  over-budget red treatment to reuse.
- **Chain origin** is the user's earliest transaction month. `openingBalance` for that month is 0.
- **No caching/denormalization for `openingBalance` yet** — explicitly declining the FE's suggestion as
  premature. The fold is over *months*, not transactions (one grouped query, a few dozen rows at this scale).
  Note this is the opposite call from `CorrectedAmount` in Phase 11, and the reason differs: there the
  denormalization bought uniform simplicity across five analytics handlers, here it would buy nothing but drift
  risk. Revisit only if a user crosses ~5 years of history or it shows up in profiling.
- **Recurring materialization: auto-create, via the existing background-task pattern** (owner's decision —
  no confirmation queue; a pending inbox that goes unserviced leaves the data just as wrong while adding a
  state machine).
- **Generation catches up to the latest state, modelled on `ProcessOccurrencesTask` / `GenerateMissingOccurrences`**
  (owner's steer). Mirror that precedent's shape end to end:
  1. **Repository pre-filters candidates in SQL** so the handler never loads templates with nothing to do —
     the quest version selects only repeatables whose latest occurrence end is null or in the past.
  2. **The catch-up span is derived, not scheduled**: quests compute `fromDate` from the last existing
     occurrence, falling back to `StartDate ?? CreatedAt`, then generate every window up to `now`.
  3. **Existence check before insert**, so a second run in the same process is a no-op.
  4. Handler loops candidates, accumulates a generated count, **one** `SaveChangesAsync`, returns affected
     rows; the task logs started / count / finished inside try-catch.
  Consequence, same as the quest tasks: it is correct whenever it happens to fire and harmless if it fires
  twice, so it needs no scheduler.
  ⚠️ `StartupTask` has **no timer** — it runs once per process start, so generation is tied to restarts. This
  is not a new finance-specific risk: `ResetQuestsTask` and `ProcessOccurrencesTask` already carry exactly the
  same tradeoff app-wide, and matching them keeps one story. If it ever matters, the catch-up design means
  promoting all of them to a `PeriodicTimer`-based `BackgroundService` is contained and changes **no** API
  contract.
- **One deliberate deviation from the quest precedent: keep a `LastMaterializedOn` watermark.** Quests dedupe
  purely by checking whether a matching occurrence row already exists. That is right for quests, where an
  occurrence is a system-generated window no one deletes — but a transaction is a **user-owned record they
  absolutely will delete**. Under pure existence-checking, deleting the August "Netflix 40" row means the next
  restart silently recreates it, which reads as a bug. The watermark is what distinguishes *never generated*
  from *generated and then deleted*. Keep `FinanceTransaction.RecurringTransactionId` too: the FK answers
  *where did this come from* (provenance, "from a template" in the UI) and gives a within-run existence check;
  the watermark answers *how far have we advanced*.
- **Aggregate-boundary note — do not force the quest shape here.** `Quest` owns its `QuestOccurrences`, so
  `Quest.GenerateMissingOccurrences` can construct children in-place. `RecurringTransaction` does **not** own
  `FinanceTransaction` — they are separate aggregates. So the split is: a pure calculator returns the missing
  months, and the handler constructs the transactions via `FinanceTransaction.CreateRecurring(...)` and advances
  the watermark. Calculation stays in Domain; cross-aggregate construction stays in the handler.
- **Horizon:** materialize up to the current month only. Never pre-create future months.
- **A template never materializes the month it was created in** (settled 2026-07-31). `Create` stamps
  `LastMaterializedOn` to the creation month, so catch-up begins the *following* month. Contract line for the
  FE: *"a recurring template starts producing rows the month after it is created."*
  *Rationale:* the FE's "Repeat monthly" checkbox posts the real transaction **and** the template in one submit
  (`add-transaction-modal.tsx:200-205`), and that transaction carries no `RecurringTransactionId`, so the
  provenance-based dedupe below cannot see it. Materializing the creation month would therefore duplicate that
  row every time. The backend cannot distinguish that path from a standalone create in the templates modal, so
  one rule covers both; a standalone template that should also cover the current month is served by the client
  posting that first transaction itself — exactly what the checkbox path already does. Rejected alternatives:
  materializing inside `POST` (needs an FE change to stop double-posting) and an optional
  `materializeCurrentMonth` flag (extra contract surface the FE's `ICreateRecurringTransactionRequest` lacks).
- **Deleting a template must not delete its materialized rows** — the FK stays `Restrict` at the database, and
  the delete handler **nulls `RecurringTransactionId`** on the template's rows in the same unit of work. Same
  posture as `ParentCategoryId` on categories and as the Phase 11 correction cascade: the app layer does the
  work and returns a friendly error. `Restrict` on its own would make the delete fail outright for any template
  old enough to have materialized anything — which is every template worth deleting, and `DELETE` is one of only
  two things the FE's templates modal does.
- **Corrections are always created `IsPaid = true`.** Follows from the default-`true` rule above
  (`CreateCorrection` is not the recurring path); money that has come back has come back. Worth stating because
  `AddCorrectionRequest` carries no `isPaid` and the FE renders corrections nested inside the parent row.
- **FE surface confirmed (2026-07-31):** `recurring-transactions-modal.tsx` is the per-user template manager
  (list + delete), and `finance-api.ts:185-217` already defines all four endpoints. `PUT` is exported but not
  yet consumed by any component, so it ships ahead of its UI — build it to the contract anyway.
- **Materialized rows are ordinary transactions** — editable, deletable, correctable. Deleting one does not
  delete the template; deleting the template does not delete already-materialized rows.
- **`dayOfMonth` beyond the month's length clamps to the last day** (31 → 28/29/30), matching the FE's existing
  `remapOccurredOnToMonth`.
- **Module boundary:** Finance stays independent of Quests. **No gamification hooks in this phase** — no
  `IBadgeAwardingStrategy` additions, no XP/coin coupling. The only cross-module dependency remains
  `UserProfileId` for identity. Room is left for a future integration; nothing is built toward it.
- **Deprioritized by the owner (do not start):** receipt attachments, wallets/transfers, multi-currency,
  gamification. They stay in the Phase 10 backlog.

### 12.0 — Bug fix: parent-category budgets ignore sub-category spending — ✅ DONE (2026-07-31; +6 tests, 142 finance total)

Found while planning; this is a live defect, not the parked open question #1. Verified against prod:

```
100 spent on "Prąd" (id 103, sub of Mieszkanie) · 1000 budget on "Mieszkanie" (id 1)
GET budget-progress   → spent = 0 / 1000        ← Statistics screen
GET monthly-summary   → Prąd 100, parent = 1    ← Dashboard rolls up to 100 / 1000
```

`GetBudgetProgressQueryHandler` matches `t.CategoryId == budget.CategoryId` exactly, while the FE rolls up via
`collectCategoryIds`. **Dashboard and Statistics therefore disagree today**, and any budget on a parent reads
as permanently unspent — which is most budgets, given 6 parents vs 47 subs.

- [ ] Roll sub-category spending into a parent's budget. `categoriesById` is already loaded in the handler, so
      this is a matching-rule change, not new plumbing.
- [ ] Document the consequence: budgeting a parent *and* one of its subs counts that sub's spending toward
      both. Correct nested-envelope behaviour, but it should be a written decision.
- [ ] Close parked open question #2 (breakdown roll-up) as **leave as-is** — per-assigned-category plus
      `parentCategoryId` is right; the FE rolls up where it wants and Statistics deliberately does not.

### 12.1 — `isPaid` — ✅ DONE (2026-07-31; migration `20260731193752_AddTransactionIsPaid`, unapplied; +14 tests, 156 finance total)

- [ ] `Domain/Models/FinanceTransaction.cs` — `bool IsPaid { get; private set; }`, defaulting `true`;
      `MarkPaid(bool)` intention-revealing method.
- [ ] `Configuration/FinanceTransactionConfiguration.cs` — NOT NULL, default `true`.
- [ ] Migration `AddTransactionIsPaid` — additive; existing rows backfill to `true` via the column default.
- [ ] `CreateTransactionRequest` / `UpdateTransactionRequest` — optional `bool? IsPaid` (default `true` when
      omitted, per the decision above).
- [ ] New slice `Transactions/Commands/UpdatePaidStatus/` → `PATCH /api/finance/transactions/{id}/paid-status`,
      body `{ isPaid }`, returns the updated `TransactionDto`. A sub-resource verb is consistent with
      `POST /{id}/corrections`. (Correction: `PATCH` is **not** new here — `UserGoalController`,
      `NotificationController`, `QuestsController` and `FriendInvitationController` already use `[HttpPatch]`;
      match their style.)
- [ ] `TransactionDto` — expose `IsPaid`. It is already returned inside nested `corrections` for free.

### 12.2 — `openingBalance` — ✅ DONE (2026-07-31; no migration needed; +17 tests, 173 finance total)

- [ ] `IFinanceTransactionRepository.GetMonthlyTotalsAsync(userProfileId, upToYear, upToMonth, ct)` — one
      grouped query returning `(Year, Month, Type, Sum)`, summing `Amount - CorrectedAmount` over rows where
      `CorrectsTransactionId == null`. Both are plain column expressions, so this translates to SQL — do **not**
      load transaction rows and fold in memory.
- [ ] `Domain/Calculators/OpeningBalanceCalculator.cs` — pure fold over ordered monthly totals; no DB, fully
      unit-testable. Covers: no history → 0, gap months, negative carry, first month.
- [ ] `MonthlySummaryDto.OpeningBalance` + wire into `GetMonthlySummaryQueryHandler`.
- [ ] Corrections interaction is already handled by summing net amounts. Per the `isPaid` decision, **unpaid
      expenses reduce the carried balance** — intended, since an unpaid transaction counts as if already paid.
      Note it in the DTO docs so it is not later mistaken for a bug.

### 12.3 — Recurring transactions — ✅ DONE (2026-07-31; migration `20260731202128_AddRecurringTransactions`, APPLIED; +35 tests, 208 finance total)

- [ ] `Domain/Models/RecurringTransaction.cs` — `UserProfileId`, `Type`, `CategoryId?`, `Amount`, `Note?`,
      `DayOfMonth` (1–31), `IsActive`, `LastMaterializedOn` (DateOnly?), audit fields. `Create` factory,
      private setters, behaviour methods; `Amount > 0` and `DayOfMonth` range enforced in the entity.
- [ ] `Domain/Calculators/RecurrenceCalculator.cs` — pure, DB-free: given a template and today, the list of
      missing months (watermark → current month, exclusive of the future) and the clamped `DateOnly` for each.
      This is the `QuestWindowCalculator` analogue. Unit-test Feb (28/29), 30- and 31-day months, and a template
      created mid-month — which per the decision above yields **no** row for the creation month. The watermark is
      never empty in practice (`Create` stamps it), but cover the null case anyway as a defensive default.
- [ ] `FinanceTransaction.RecurringTransactionId` (nullable FK, `Restrict`, indexed) + a `CreateRecurring`
      factory that stamps provenance and sets `IsPaid = false` for expenses.
- [ ] `DELETE /{id}` handler — null `RecurringTransactionId` on the template's materialized rows before removing
      the template (see the decision above); the rows themselves survive as ordinary transactions.
- [ ] Persistence: configuration, `DbSet`, `RecurringTransactionRepository : BaseRepository<T>`, exposed on
      `IUnitOfWork`. Migration `AddRecurringTransactions` (covers both the new table and the FK column).
- [ ] Slice `Application/Finance/RecurringTransactions/` — `GET` (list), `POST`, `PUT`, `DELETE` at
      `/api/finance/recurring-transactions[/{id}]`, matching the FE's existing calls. Handlers do ownership +
      category/type consistency checks (reuse the `CreateTransaction` rule) → `NotFound`/`Conflict`; validators
      cover field rules.
- [ ] `PUT` accepts **partial** updates (`amount?`, `note?`, `dayOfMonth?`, `isActive?`) per the FE contract.
      This diverges from `PUT /transactions` (full replacement) but matches `UpdateBudgetRequest` — document it
      rather than fight it.
- [ ] `IRecurringTransactionRepository.GetForMaterializationAsync(today, ct)` — **pre-filter in SQL**, mirroring
      `GetRepeatableQuestsForOccurrencesProcessingAsync`: active templates only, where `LastMaterializedOn` is
      null or earlier than the first day of the current month. The handler should never load a template with
      nothing to do.
- [ ] `GenerateRecurringTransactionsCommand` + handler returning `int` (affected rows, like
      `GenerateMissingOccurrencesCommand`) — for each candidate: ask `RecurrenceCalculator` for the missing
      months, skip any month that already has a row for that template (`RecurringTransactionId` +
      `OccurredOn` in range), create via `FinanceTransaction.CreateRecurring(...)` with `IsPaid = false` for
      expenses, advance `LastMaterializedOn`. Accumulate the count; **one** `SaveChangesAsync` at the end.
- [ ] `Api/BackgroundTasks/GenerateRecurringTransactionsTask : StartupTask` — mirror `ProcessOccurrencesTask`
      line for line (scope factory, try/catch/finally, "started / affected rows / finished" logging). Register
      in `Program.cs` alongside the other hosted services — **the one place this phase needs a `Program.cs`
      change.**
- [ ] Use the injected `IClock` for "today", not `SystemClock.Instance` — `GenerateMissingOccurrencesCommandHandler`
      reaches for the static and ARCHITECTURE §9 already flags that as the wrong habit. Do not copy it; it also
      makes the catch-up untestable without time travel.

### 12.4 — Tests

- [ ] Pure domain/calculator tests with **no DB**: `RecurrenceCalculator` (month-length clamping),
      `OpeningBalanceCalculator` (gaps, negatives, empty history), `FinanceTransaction.MarkPaid`.
- [ ] Handler tests: paid-status round-trip; recurring CRUD ownership; **generation idempotency** — running
      the command twice produces one row per template-month; a **user-deleted occurrence is not resurrected**
      (the case the watermark exists for, and the one that would regress if someone later "simplifies" it to
      the quests' pure existence-check); and a multi-month catch-up after a long gap backfills every month.
- [ ] Budget roll-up regression test for 12.0 (spend on a sub, budget on the parent).
- [ ] ⚠️ 12.2 and 12.3 are date-boundary logic, exactly where the InMemory provider (ARCHITECTURE §10) will
      pass while real SQL differs — especially the grouped `SUM` in 12.2. Keep the logic in pure calculators so
      most cases need no DB, and consider SQLite-in-memory for the repository-level tests.

### 12.5 — Contract handoff

- [ ] Update `docs/finance-api-schema.ts` (the live FE contract) with `isPaid`, `openingBalance`, the recurring
      DTOs and the two new routes.
- [ ] Regenerate `docs/swagger.json` — still missing from the repo (see Status).

---

## Build / verify

```
dotnet build GoodieHabits.sln
dotnet test Application.Tests
dotnet ef migrations add AddFinanceModule --project Infrastructure --startup-project Api
```
