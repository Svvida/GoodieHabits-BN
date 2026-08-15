# GoodieHabits — Backend Architecture

> Knowledge base for humans and LLMs onboarding to this repository. Read this before making changes so you understand *where* code belongs and *which* conventions to follow. Keep it up to date when structural decisions change.

---

## 1. What this is

GoodieHabits is the **backend API for a gamified habit/quest-tracking mobile application**. Users create *quests* (one-time or repeating habits) and *goals*, complete them to earn **XP** and **coins**, **level up**, unlock **badges**, spend coins in a **shop**, manage an **inventory** of cosmetic/consumable items, and interact **socially** (friends, invitations, blocking, leaderboards). Real-time updates (e.g. level-up, notifications) are pushed over SignalR.

It is a REST API (ASP.NET Core, controllers) consumed by a mobile client. There is no server-rendered UI; Swagger is exposed for exploration.

---

## 2. Solution shape & architectural style

The solution (`GoodieHabits.sln`) is a blend of **Clean/Onion Architecture** (concentric layers, dependencies point inward) and **Vertical Slice Architecture** (the Application layer is organized by *feature*, not by technical role).

```
┌──────────────────────────────────────────────────────────┐
│  Api            (ASP.NET Core host, controllers, DI setup) │  ── depends on ──▶ Application, Infrastructure
├──────────────────────────────────────────────────────────┤
│  Infrastructure (EF Core, external services, repos)        │  ── depends on ──▶ Application, Domain
├──────────────────────────────────────────────────────────┤
│  Application    (CQRS handlers, validation, mapping, DTOs) │  ── depends on ──▶ Domain
├──────────────────────────────────────────────────────────┤
│  Domain         (entities, value objects, enums, rules)    │  ── depends on ──▶ (nothing but NodaTime)
└──────────────────────────────────────────────────────────┘

  Application.Tests  ── references ──▶ Application, Domain, Infrastructure
```

### Project responsibilities

| Project | Responsibility | Key rule |
|---|---|---|
| **Domain** | Business entities, value objects, enums, domain events, pure business calculators, exception hierarchy, and **interface contracts** (repositories, `IUnitOfWork`, auth token generators, etc.). | No dependency on any other project. Only external package: **NodaTime**. Contains no EF Core / framework code. |
| **Application** | Use cases as CQRS **commands/queries + handlers**, FluentValidation validators, Mapster mapping profiles, DTOs, orchestration services (badge awarding, notifications), and application-level interfaces. | Depends only on Domain. Knows *what* to do, not *how* infrastructure is implemented. |
| **Infrastructure** | Concrete implementations of Domain/Application interfaces: EF Core `AppDbContext`, entity configurations, migrations, repositories + `UnitOfWork`, JWT, email (MailKit), photos (Cloudinary), SignalR notification transport, nickname generator. | Implements interfaces defined inward. Never referenced by Domain/Application. |
| **Api** | Composition root. HTTP controllers (thin), middleware, JSON converters, JWT/claims helpers, background hosted services, and **all DI wiring** in `Program.cs`. | The only executable. Wires interfaces to implementations. |
| **Application.Tests** | xUnit test suite, currently focused on Application-layer handlers/validators using EF Core InMemory. | See §10 for the state of the tests and the planned refactor. |

**Dependency rule:** dependencies always point inward (toward Domain). If you find yourself wanting Domain or Application to reference Infrastructure, define an interface inward and inject the implementation from `Api/Program.cs` instead.

---

## 3. Tech stack

- **Runtime:** .NET 8 (all projects `net8.0`), C# with `Nullable` and `ImplicitUsings` enabled. Namespaces are block-scoped; primary constructors are used widely.
- **Web:** ASP.NET Core (controllers), Swashbuckle/Swagger, SignalR.
- **Mediation / CQRS:** MediatR 13.
- **Validation:** FluentValidation 12 (via a MediatR pipeline behavior).
- **Mapping:** Mapster 7 (`IMapper` = `MapsterMapper.ServiceMapper`). *Not* AutoMapper.
- **Persistence:** EF Core 9, SQL Server provider. Repository + Unit of Work over `DbContext`.
- **Time:** NodaTime (`IClock` / `SystemClock`) — prefer this over `DateTime.Now`.
- **Auth:** JWT bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), ASP.NET Core Identity `PasswordHasher<Account>`.
- **External services:** Cloudinary (avatars/images), MailKit (email, e.g. password reset), SignalR (real-time push).
- **Logging:** Serilog.
- **Images:** SixLabors.ImageSharp (upload validation).
- **Tests:** xUnit, Moq, FluentAssertions, EF Core InMemory.

---

## 4. CQRS & the request pipeline

Every use case is a **command** (mutation) or **query** (read), handled by MediatR.

Marker interfaces (`Application/Common/Interfaces/`):
```csharp
public interface ICommand : IRequest<Unit> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface IQuery<out TResponse> : IRequest<TResponse> { }
```

**Cross-cutting pipeline behavior:** `Application/Common/Behaviors/ValidationBehavior.cs` runs *before* every handler. It resolves all `IValidator<TRequest>` for the request, runs them, and throws `Application.Common.Exceptions.ValidationException` (aggregating failures) if any fail. This is registered in `Program.cs` as an `IPipelineBehavior<,>`. **Consequence:** validators are applied automatically at runtime, but *not* when a handler is invoked directly in a unit test (see §10).

### End-to-end request lifecycle (example: create a quest label)

1. **Controller** (`Api/Controllers/QuestLabelController.cs`) — thin. Injects `ISender` + `IMapper`. Maps the inbound `...Request` to a command, injects identity from the JWT via `User.GetCurrentUserProfileId()` (extension in `Api/Helpers/JwtHelpers.cs`), then `sender.Send(command)`.
2. **ValidationBehavior** runs the matching `...CommandValidator`.
3. **Handler** (`Application/.../CreateQuestLabelCommandHandler.cs`) — orchestrates: calls a Domain factory (`QuestLabel.Create(...)`), persists via `IUnitOfWork`, `SaveChangesAsync`, maps the entity to a `...Response`/DTO with `IMapper`, returns it.
4. **Repository / UnitOfWork** (Infrastructure) executes EF Core operations.
5. **ExceptionHandlingMiddleware** (`Api/Middlewares/`) translates thrown exceptions to HTTP responses.

Identity is carried in two custom JWT claims — `AccountId` and `UserProfileId` (`Domain/ValueObjects/JwtClaimTypes.cs`). Controllers pass `UserProfileId` (the game-domain identity) into most commands/queries. A custom `x-time-zone` header (IANA format) conveys the user's timezone.

---

## 5. Vertical slice layout (Application layer)

The Application project is organized **by feature domain**, then by CQRS role. A typical slice folder contains all the small files a use case needs, co-located:

```
Application/
  Quests/                                  ← feature domain
    Commands/
      CreateQuest/
        CreateQuestCommand.cs              ← record : ICommand<TResponse>
        CreateQuestCommandHandler.cs       ← IRequestHandler
        CreateQuestCommandValidator.cs     ← AbstractValidator (may live under Validators/)
        CreateQuestMappingProfile.cs       ← Mapster IRegister
        CreateQuestRequest.cs              ← API-facing input DTO (mapped to command in controller)
        Handlers/                          ← per-quest-type derived handlers
        Validators/                        ← per-quest-type validators
    Queries/
      GetQuestById/ ...
    Dtos/            ← response DTOs shared across the feature
    Mappings/        ← shared mapping profiles
    Utilities/       ← feature-specific helpers
```

Naming conventions you can rely on:
- Command/Query: `XxxCommand` / `XxxQuery` (usually a `record`).
- Handler: `XxxCommandHandler` / `XxxQueryHandler`.
- Validator: `XxxCommandValidator` (FluentValidation).
- API input: `XxxRequest`; API output: `XxxResponse` or a `...Dto`.
- Mapping: `XxxMappingProfile` implementing Mapster's `IRegister`, auto-discovered by `TypeAdapterConfig.Scan(applicationAssembly)`.

### Feature domains present today
`Accounts`, `Auth`, `Badges`, `Finance` (§13), `FriendInvitations`, `Friendships`, `Inventories`, `Leaderboard`, `Nicknames`, `Notifications`, `QuestLabels`, `Quests`, `Shop`, `Statistics`, `Supplements` (§14), `UserBlocks`, `UserGoals`, `UserProfiles`, `Workouts` (§14), plus shared `Common`.

### Notable patterns inside slices
- **Strategy pattern**, registered as multiple DI implementations of one interface and selected at runtime:
  - **Badge awarding** — `Application/Badges/Strategies/*`, each an `IBadgeAwardingStrategy` with a `Trigger`. `BadgeAwardingService` resolves `IServiceProvider.GetServices<IBadgeAwardingStrategy>()`, filters by trigger, applies them, then dispatches resulting domain events.
  - **Friend invitation status transitions** — `FriendInvitations/Commands/UpdateInvitationStatus/Strategies/*` implementing `IInvitationStatusUpdateStrategy` (Accept/Reject/Cancel).
- **Generic base handler with virtual hook** — `CreateQuestCommandHandler<TCommand, TResponse>` holds the shared quest-creation flow; per-type handlers (`CreateDailyQuestCommandHandler`, `CreateWeeklyQuestCommandHandler`, …) derive from it and override `HandleQuestSpecificsAsync`. The same pattern exists for quest updates.

---

## 6. Domain layer details

Located in `Domain/`. This is a **rich domain model**, not anemic.

- **Entities** (`Domain/Models/`) use:
  - `static Create(...)` **factory methods** as the public construction path; a `protected` parameterless ctor exists only for EF Core; real ctors are `private`.
  - `private set` / `init` on properties — state changes go through intention-revealing methods (`Complete(...)`, `Uncomplete(...)`, `UpdateDates(...)`, `ApplyQuestCompletionRewards(...)`), which enforce invariants and throw domain exceptions.
- **Base types** (`Domain/Common/`):
  - `AggregateRoot` — holds domain events (`AddDomainEvent`, `DomainEvents`, `ClearDomainEvents`).
  - `EntityBase : AggregateRoot` — adds `CreatedAt` / `UpdatedAt` with guards; timestamps are stamped automatically in `AppDbContext.SaveChangesAsync` (`UpdateTimestamps()`).
- **Value objects** (`Domain/ValueObjects/`): `LevelInfo`, `LevelingOptions`, `QuestOccurrenceWindow`, `QuestStatisticsData`, `ShopItemPayload`, `ActiveEffectValues`, `JwtClaimTypes`, etc.
- **Enums** (`Domain/Enums/`): `QuestTypeEnum`, `DifficultyEnum`, `PriorityEnum`, `BadgeTypeEnum`, `BadgeTriggerEnum`, `ShopItemTypeEnum`, `CurrencyTypeEnum`, `FriendshipStatus`, `NotificationTypeEnum`, `SeasonEnum`, `WeekdayEnum`, etc.
- **Pure calculators** (`Domain/Calculators/`): `NextResetDateCalculator`, `QuestStatisticsCalculator`, `QuestWindowCalculator`, `QuestAnalyticsCalculator` — stateless, side-effect-free business math.
- **Domain events** (`Domain/Events/`): e.g. `QuestDeletedEvent`, `BadgeAwardedEvent`.
- **Exceptions** (`Domain/Exceptions/`): all derive from `AppException`, which carries an HTTP `StatusCode`. Examples: `NotFoundException`, `ConflictException`, `ForbiddenException`, `UnauthorizedException`, `InvalidArgumentException`, `PurchaseItemException`, `FriendshipException`. The API middleware relies on this hierarchy.
- **Interfaces** (`Domain/Interfaces/`): repository contracts, `IUnitOfWork`, `ITokenGenerator`/`ITokenValidator`, `INicknameGenerator`. These are implemented in Infrastructure.

### The Quest model (worth understanding)
`Domain/Models/Quest.cs` is a **single entity discriminated by `QuestType`** (Daily/Weekly/Monthly/OneTime/Seasonal) rather than a class hierarchy. Type-specific data lives in **satellite entities**: `MonthlyQuest_Days`, `WeeklyQuest_Day`, `SeasonalQuest_Season`. Repeatable quests (Daily/Weekly/Monthly) also own `QuestOccurrence` records and `QuestStatistics`. The entity encapsulates completion, XP/reward calculation, occurrence generation, reset logic, and statistics recalculation.

### Quest occurrences are calendar periods, not instants ⚠️

A `QuestOccurrence` is identified by **`PeriodStart`/`PeriodEnd` (`DateOnly`, SQL `date`, both inclusive)** —
the local calendar days the quest was due, *not* a UTC instant range. Same reasoning as
`FinanceTransaction.OccurredOn` (§13): a period is a fact about the user's calendar, so it must not move
when they travel and `UserProfile.TimeZone` changes (which `RefreshAccessToken` does on any token refresh).
`Quest.StartDate`/`EndDate` are `DateOnly` for the same reason.

- **Single source of truth for "what day is it":** `UserProfile.LocalDateOn(instantUtc)`. Everything that
  needs the user's today goes through it; `QuestWindowCalculator` is then pure calendar arithmetic with no
  timezone code at all.
- **`CompletedAt` stays a UTC `DateTime`** — "when the user tapped complete" is a genuine instant, and it is
  what time-of-day analytics read. `NextResetAt` is likewise a real scheduling instant.
- **`UNIQUE (QuestId, PeriodStart)`** is what makes duplicate periods structurally impossible. The previous
  index keyed on UTC instants, so a timezone change silently produced two occurrences for the same local day
  (inflating `OccurrenceCount`, inventing failures, breaking streaks). Because the DB now enforces this,
  repository queries that feed occurrence generation must `Include` **all** of a quest's occurrences — a
  partial load would let in-memory de-duplication miss and blow up on the unique index.
- **Denominator rule:** a period counts as a failure only once it has fully elapsed relative to the user's
  local today. `CompletionRate` divides by *evaluated* periods (elapsed + already completed), and is `null`
  when nothing has been evaluated, so an in-progress day never drags today's percentage down.
- **Analytics** (`Application/Quests/Queries/{GetQuestAnalytics,GetHabitsOverview}/`) aggregate in memory over
  `IQuestOccurrenceRepository.GetForQuestInRangeAsync` / `GetForUserInRangeAsync`, mirroring how the finance
  analytics slices work. `QuestStatistics` remains a denormalized cache for list views, not the query path.
- **Migration (done — historical note):** shipped as `QuestCalendarPeriods_Step1_AddColumns` (additive) → a
  one-shot `BackfillQuestOccurrencePeriodsTask` that populated the period columns and de-duplicated the
  drift damage → `QuestCalendarPeriods_Step2_Finalize` (constraints + drop of the old instant columns; it
  hard-fails if the backfill has not run). The backfill had to be C# rather than SQL inside the migration
  because SQL Server's `AT TIME ZONE` speaks Windows timezone ids while the app stores IANA ones. Both
  migrations remain in history; **the backfill task and its command/service were deleted once applied** — if
  you ever replay these migrations onto a database that still holds legacy rows, recover that code from git
  history rather than re-deriving it.

---

## 7. Infrastructure layer details

Located in `Infrastructure/`.

- **`Persistence/AppDbContext.cs`** — all `DbSet`s; applies one `IEntityTypeConfiguration` per entity from `Persistence/Configuration/*`; seeds the 20 `Badge` rows in `OnModelCreating`; overrides `SaveChanges[Async]` to stamp timestamps.
- **Repositories** (`Persistence/Repositories/`) — each derives from generic `BaseRepository<T>` (`GetByIdAsync`, `ExistsByIdAsync`, `AddAsync`, `Remove`, `ExecuteDeleteAsync`, …) and adds feature-specific queries (e.g. `QuestLabelRepository.GetUserLabelsAsync`, with `asNoTracking` flags for read paths).
- **`UnitOfWork`** (`Persistence/UnitOfWork.cs`) — single entry point exposing all repositories (lazily instantiated) and `SaveChangesAsync`. Application handlers depend on `IUnitOfWork`, **not** on individual repositories. It shares one `AppDbContext` across all repos so a handler's work commits atomically in one `SaveChangesAsync`.
- **Migrations** (`Persistence/Migrations/`) — EF Core migrations; `DesignTimeDbContextFactory` supports design-time tooling.
- **External services:**
  - `Authentication/` — `TokenGenerator`, `TokenValidator`, `JwtSettings`.
  - `Email/` — `EmailSender` (MailKit) + `Senders/ForgotPasswordEmailSender` + HTML templates.
  - `Photos/` — `CloudinaryPhotoService`, `CloudinaryUrlBuilder` (avatar upload + URL building).
  - `Notifications/` — `NotificationHub` (SignalR), `SignalRNotificationSender`, `NotificationService`, `UserProfileIdProvider` (custom `IUserIdProvider` because the app doesn't use the `sub` claim).
  - `Services/NicknameGenerator.cs` + `Utilities/WordLists.cs` — random nickname generation.

---

## 8. Api layer details

Located in `Api/`.

- **`Program.cs`** — the composition root. All DI registration lives here in `ConfigureServices`: MediatR (+ `ValidationBehavior`), FluentValidation (assembly scan), Mapster config scan, EF Core `AppDbContext` (SQL Server), `IUnitOfWork`, auth/JWT, SignalR + custom `IUserIdProvider`, all strategy implementations (badges, invitation status), external service implementations, password hasher, options binding (`JwtSettings`, `LevelingOptions`, `EmailSettings`, `CloudinarySettings`), and hosted background services. `ConfigureMiddleware` sets the pipeline order.
- **Controllers** (`Api/Controllers/`) — thin, `[ApiController]`, mostly `[Authorize]`, inject `ISender` (+ `IMapper` where needed). They map `Request → Command/Query`, attach identity from claims, `Send`, and return `Ok`/`NoContent`. **No business logic in controllers.**
- **`Middlewares/ExceptionHandlingMiddleware.cs`** — maps exceptions to HTTP: `ValidationException` → 400 (with field errors), `AppException` → its `StatusCode`, `SecurityTokenException` → 401, anything else → 500. Registered first in the pipeline.
- **`BackgroundTasks/`** — `IHostedService`s deriving from `StartupTask`. They create a DI scope and dispatch a MediatR command: `ResetQuestsTask`, `ExpireGoalsTask`, `ProcessOccurrencesTask`, `RecalculateRepeatableQuestStatisticsTask`, `GenerateRecurringTransactionsTask`. This keeps scheduled/maintenance work expressed as ordinary application use cases.
- **`Converters/`** — custom `System.Text.Json` converters (UTC `DateTime`, `TimeOnly`, string trimming). Registered in `AddControllers().AddJsonOptions(...)` along with `JsonStringEnumConverter` (enums serialize as strings).
- **`Helpers/`** — `JwtHelpers` (claims → ids as `ClaimsPrincipal` extensions), `ImageValidator`.

---

## 9. Cross-cutting concerns

- **Validation:** FluentValidation, applied automatically via `ValidationBehavior`. Custom rules in `Application/Common/ValidatorsExtensions/`.
- **Mapping:** Mapster. Profiles implement `IRegister`; the config is built once via assembly scan and injected as `IMapper`. Controllers map `Request → Command`; handlers map `Entity → Response/Dto`.
- **Time:** NodaTime `IClock` registered as `SystemClock.Instance`. ⚠️ Some code (e.g. `CreateQuestCommandHandler`) calls `SystemClock.Instance` directly instead of the injected `IClock` — prefer injecting `IClock` for testability.
- **Domain events:** dispatched **manually** where needed — e.g. `BadgeAwardingService` and `DeleteQuestCommandHandler` iterate `entity.DomainEvents`, wrap each via `DomainEventsHelper.CreateDomainEventNotification`, `IPublisher.Publish` them, then `ClearDomainEvents()`. There is **no central dispatch** in `SaveChanges`, so raising an event requires a handler to remember to publish and clear it (see §10).
- **Real-time:** SignalR hub at `/api/hubs/notifications`; token accepted via `access_token` query string for the hub. `INotificationService`/`INotificationSender` abstract sending.
- **Errors:** exception-based flow — throw `AppException` subclasses from Domain/Application; the middleware shapes the HTTP response.
- **Config/secrets:** `appsettings.json` + environment-specific overrides + environment variables; strongly-typed via `IOptions<T>`.

---

## 10. Testing — current state & known gaps

`Application.Tests` covers Application-layer handlers/validators with xUnit + Moq + FluentAssertions, using **EF Core InMemory**. It is the area most in need of a refactor. Current shape and issues:

**`TestBase<THandler>`** (`Application.Tests/TestBase.cs`) is a shared base that:
- spins up an InMemory `AppDbContext` (unique DB per test) and a real `UnitOfWork`;
- pre-builds a large fixed set of mocks (`IClock`, `ILevelCalculator`, `IUrlBuilder`, `INotificationSender`, `IMediator`, email sender, …) and a real Mapster `IMapper`;
- exposes many `AddXxxAsync` seed helpers.

**Pain points to address in the refactor:**
1. **InMemory provider ≠ SQL Server.** It ignores relational constraints, cascade/`ExecuteDelete` semantics, `HasData` seeding nuances, and query translation differences — tests can pass while real SQL behavior differs. Consider SQLite in-memory (relational) or Testcontainers-backed SQL Server for persistence-touching tests, and pure unit tests (no DB) for logic that doesn't need one.
2. **God-object base class.** `TestBase<THandler>` forces every test to construct every mock and seed helper regardless of need, and couples the generic parameter to a single handler. Prefer small, composable fixtures / builders and xUnit `IClassFixture` where shared setup is genuinely needed.
3. **Inconsistent test-data construction.** Some tests use domain factories via `TestBase` (`GetOrCreateShopItemAsync` → `ShopItem.Create`), while others build entities through **reflection on private setters** (`PurchaseItemCommandHandlerTests.AddShopItemAsync` / `SetPrivateProperty`). Reflection-based construction is brittle and bypasses invariants. Standardize on **Object Mother / Builder** helpers backed by the real factories.
4. **Pipeline not exercised.** Handlers are `new`-ed up directly, so the `ValidationBehavior` (and thus validators) never runs in these tests. Validator behavior needs its own dedicated tests, and/or a way to test the full MediatR pipeline.
5. **Coverage gaps.** Domain calculators, quest occurrence/reset logic, and background-task command handlers deserve focused unit tests independent of the DB.

When refactoring, aim for: a clear split between **pure unit tests** (Domain logic, calculators, handlers with mocked `IUnitOfWork`) and **integration tests** (relational DB), consistent builders for test data, and per-feature fixtures instead of one monolithic base.

---

## 11. Recipe: add a new feature slice

To add a use case (the **finance module**, §13, is the most recent worked example of this recipe end to end):

1. **Domain first (if new concepts):** add entity/value-object/enum under `Domain/`, with a `Create` factory, private setters, and behavior methods that enforce invariants. Add a repository interface under `Domain/Interfaces/Repositories/` and register it on `IUnitOfWork` if it needs persistence.
2. **Persistence:** add an `IEntityTypeConfiguration` under `Infrastructure/Persistence/Configuration/`, a `DbSet` in `AppDbContext`, a repository under `Infrastructure/Persistence/Repositories/` (derive from `BaseRepository<T>`), wire it into `UnitOfWork`, and **add an EF migration**.
3. **Application slice:** create `Application/<Feature>/Commands|Queries/<UseCase>/` with `Command`/`Query` (record), `Handler`, `Validator`, `Request`/`Response` or `Dto`, and a Mapster `MappingProfile` (`IRegister`). Depend on `IUnitOfWork` and `IMapper`. Throw `AppException` subclasses for error paths.
4. **Api:** add/extend a thin controller in `Api/Controllers/` that maps request → command/query, injects identity via `User.GetCurrentUserProfileId()`, and `Send`s it. Register any *new* interfaces/implementations in `Program.cs`.
5. **Tests:** add handler tests (and validator tests) following the conventions in §10 — prefer builders over reflection.

**Conventions checklist:** block-scoped namespaces; primary constructors; `record` commands/queries; `Xxx{Command,Query,Handler,Validator,Request,Response,Dto,MappingProfile}` naming; NodaTime for time; no business logic in controllers; dependencies point inward; one `SaveChangesAsync` per unit of work.

---

## 12. Quick file map

| Looking for… | Go to |
|---|---|
| DI wiring / service registration | `Api/Program.cs` |
| HTTP → status-code mapping | `Api/Middlewares/ExceptionHandlingMiddleware.cs` |
| Claims / current user id | `Api/Helpers/JwtHelpers.cs`, `Domain/ValueObjects/JwtClaimTypes.cs` |
| Scheduled/maintenance jobs | `Api/BackgroundTasks/` |
| CQRS marker interfaces | `Application/Common/Interfaces/` |
| Validation pipeline | `Application/Common/Behaviors/ValidationBehavior.cs` |
| A use case's logic | `Application/<Feature>/Commands|Queries/<UseCase>/` |
| Business rules / invariants | `Domain/Models/*`, `Domain/Calculators/*` |
| Error types & status codes | `Domain/Exceptions/*` (all `: AppException`) |
| DB schema / mappings | `Infrastructure/Persistence/AppDbContext.cs` + `Configuration/*` |
| Data access | `Infrastructure/Persistence/Repositories/*`, `UnitOfWork.cs`; contracts in `Domain/Interfaces/Repositories/*` |
| External integrations | `Infrastructure/{Authentication,Email,Photos,Notifications,Services}/` |
| Test setup | `Application.Tests/TestBase.cs` |
| Finance module (detail) | `docs/finance-module.md` (decisions, model rules, API surface, seeding hazards) |
| Workouts & Supplements (detail) | `docs/workouts-module.md` (decisions, model rules, API surface, seeding hazards) |

---

## 13. Finance module

A personal-finance feature domain — income/expense tracking, hierarchical categories, budgets, analytics,
transaction corrections and monthly recurring templates — built as a standard vertical slice under
`Application/Finance/` and keyed on **`UserProfile`** (not the auth `Account`). It follows every convention above.
**Core is built and green** (222 finance tests). This section is the overview; the decisions and their rationale
live in **[`docs/finance-module.md`](./docs/finance-module.md)** — read it before changing any of the rules below,
several of which are load-bearing in non-obvious ways.

- **Entities** (`Domain/Models/`): `FinanceTransaction` (single entity discriminated by `FinanceTransactionTypeEnum`,
  mirroring `Quest`), `FinanceCategory` (self-referencing, exactly two levels; system rows seeded via `HasData`
  with `UserProfileId = null`, like `Badge`), `Budget` (overall or per-category, monthly/yearly),
  `RecurringTransaction` (a monthly template, never itself an aggregate row).
- **Money & time:** amounts are `decimal(18,2)`, always positive (sign implied by type). A transaction's
  `OccurredOn` is a **`DateOnly`** (SQL `date`) — a calendar fact, deliberately *not* a UTC instant — which
  sidesteps timezone/month-boundary bugs in analytics (§6 applies the same reasoning to quest occurrences). Audit
  timestamps stay UTC. Currency is a single ISO-4217 string per user on `UserProfile.Currency` (allow-list in
  `Domain/ValueObjects/SupportedCurrencies.cs`).
- **Category inheritance:** a sub-category derives both `Type` and `IsSavings` from its parent — values sent for a
  sub are ignored, and changing `IsSavings` on a main cascades. `IsSavings` and `IsPaid` are both **pure
  metadata**: analytics treat savings rows, and unpaid rows, exactly like any other. Presentation is the client's.
- **Corrections** are a *relation*, not a type: a refund/payback is a `FinanceTransaction` pointing at its parent
  via `CorrectsTransactionId` and inheriting the parent's `Type` and `CategoryId` (the reserved
  `FinanceTransactionTypeEnum.Transfer` slot stays reserved for wallets). Netting is materialized on the parent,
  so **every analytics query sums `NetAmount = Amount - CorrectedAmount`**, and one repository filter
  (`CorrectsTransactionId == null`) keeps corrections out of all five of them.
- **Recurring generation** follows the quest-task precedent: SQL-pre-filtered candidates, derived catch-up span,
  one `SaveChangesAsync`, dispatched from `Api/BackgroundTasks/GenerateRecurringTransactionsTask` (the module's
  only `Program.cs` registration). It deviates on one point — a `LastMaterializedOn` watermark rather than pure
  existence-checking, because a materialized transaction is a user-owned record they may delete and must not have
  resurrected.
- **Calculators** (`Domain/Calculators/`, pure and DB-free): `FinancePeriodCalculator` (timezone-free `DateOnly`
  bounds), `BudgetProgressCalculator`, `OpeningBalanceCalculator` (signed running balance, no clamp),
  `RecurrenceCalculator` (missing months + day-of-month clamping).
- **Slices:** `Application/Finance/{Categories,Transactions,Budgets,Analytics,RecurringTransactions,Settings}/`.
  Relational checks (ownership, category/type consistency, uniqueness) live in handlers (→ `NotFound`/`Conflict`);
  validators cover field rules. Analytics aggregate in memory over
  `IFinanceTransactionRepository.GetForPeriodAsync`; only the opening-balance totals are a grouped SQL projection.
- **API:** thin controllers under `/api/finance/{categories,transactions,budgets,analytics,recurring-transactions,settings}`,
  plus the sub-resource verbs `POST /transactions/{id}/corrections` and `PATCH /transactions/{id}/paid-status`.
  Handlers/validators/Mapster profiles are picked up by the existing assembly scans and all four repositories are
  exposed through `IUnitOfWork`.
- **FE contract:** hand-written TypeScript types in `docs/finance-api-schema.ts` (enums serialize as strings,
  `DateOnly` → `"YYYY-MM-DD"`), alongside the generated `docs/swagger.json`.
- ⚠️ **Seeded category ids share one IDENTITY sequence with user rows.** The identity is reseeded to 100 000 —
  system categories below, user categories above; ids 149–157 are burned. Number new system categories from 167
  up, and expect the same hazard in any other table mixing `HasData` with user data.
- **Deferred (designed-for, not built):** multiple wallets + transfers, multi-currency (`Money` VO + FX),
  gamification hooks, receipt attachments. Finance stays independent of Quests.

---

## 14. Workouts & Supplements modules

Two **sibling** feature domains — training (exercise library, routines, performed sessions, analytics) and
supplements (catalog, schedule, daily checklist, adherence) — both keyed on **`UserProfile`** and following
every convention above. **Core is built and green** (208 tests inside 668 total). This section is the
overview; the decisions and their rationale live in **[`docs/workouts-module.md`](./docs/workouts-module.md)**
— read it before changing any of the rules below.

- **They are siblings, not parent/child.** The only link is a nullable `WorkoutSessionId` on a supplement
  intake. This is forced by the product requirement that a supplement plan works on a rest day, so it cannot
  hang off a session; the in-training panel is `GET /supplements/checklist` filtered to
  `PreWorkout`/`PostWorkout`, and *that filter is the entire integration*. Same posture as Finance vs Quests.
- **Entities** (`Domain/Models/`): `Exercise` (single entity discriminated by `ExerciseMetricEnum`, mirroring
  `Quest`; system rows seeded via `HasData` with `UserProfileId = null`), `WorkoutRoutine` +
  `WorkoutRoutineExercise` (template), `WorkoutSession` + `WorkoutSessionExercise` + `WorkoutSet` (the
  aggregate that records what happened), `Supplement` + `SupplementScheduleSlot` + `SupplementIntake`,
  plus the `ExerciseBest` projection type.
- **Time:** `PerformedOn` / `TakenOn` are `DateOnly` (SQL `date`) — calendar facts, for the same reason as
  `FinanceTransaction.OccurredOn` and quest occurrence periods (§6). `StartedAt` / `CompletedAt` / `TakenAt`
  stay UTC instants. "Today" always comes from `UserProfile.LocalDateOn`.
- **Weight unit is one setting per user** (`UserProfile.WeightUnit`, default `kg`) and changing it **never**
  converts stored values — the `UserProfile.Currency` call, repeated.
- **Template vs record:** starting a session *copies* the routine's exercises and snapshots each exercise's
  name and metric. Editing or deleting a routine never reaches a session already performed from it (deleting
  nulls `RoutineId`, `Restrict` + handler). Identical to `RecurringTransaction → FinanceTransaction`.
- **Metric declares what a set *requires*, never what it forbids** — weight on a `Reps` exercise is a
  weighted pull-up, not an error. The check lives in the entity (it needs the entry's snapshotted metric), so
  it surfaces as an `AppException`-driven 400 rather than a FluentValidation failure.
- **Two logging paths, one row shape:** granular `POST .../sets` for live logging, and
  `PUT /sessions/{id}/log` as **full replacement** so an offline sync retry is idempotent. Every mutating
  session endpoint returns the whole session.
- **At most one `InProgress` session per user** — filtered unique index on `Status = 0`, with the handler
  turning it into a 409 that names the active session.
- **`UNIQUE (ScheduleSlotId, TakenOn)`** (filtered) makes double-ticking a dose structurally impossible — the
  `UNIQUE (QuestId, PeriodStart)` lesson applied up front. The checkbox is an idempotent `PUT` carrying
  intent; ad-hoc doses are a separate, deliberately repeatable `POST`.
- **Calculators** (`Domain/Calculators/`, pure and DB-free): `WorkoutVolumeCalculator`, `OneRepMaxCalculator`
  (Epley), `SupplementAdherenceCalculator` (reuses the quest elapsed-period denominator rule).
- **Gamification hook ships, rewards do not:** `WorkoutSessionCompletedEvent` +
  `BadgeTriggerEnum.WorkoutSessionCompleted` are raised and published on finish (never on abandon); no
  `IBadgeAwardingStrategy` consumes them yet, so adding XP/coins later needs no migration.
- **API:** thin controllers under `/api/workouts/{exercises,routines,sessions,analytics,settings}` and
  `/api/supplements{,/checklist,/intakes,/analytics}`. **No `Program.cs` registration was needed** — the
  existing assembly scans pick everything up and all five repositories hang off `IUnitOfWork`.
- **FE contract:** `docs/workouts-api-schema.ts` (hand-written TS) + `docs/swagger.json`, with the FE guide in
  `docs/trening-frontend.md`.
- ⚠️ **`Exercises` mixes `HasData` ids with user rows on one IDENTITY sequence.** The table's first migration
  reseeds the identity to **100 000** — the fix that was retrofitted to `FinanceCategories` after a production
  `PK` collision, applied here from day one. System exercises live below it in documented per-muscle id
  blocks; never seed at or above 100 000.
- ⚠️ **Grouped projections are where InMemory lies.** `GetPersonalRecordsAsync` was first written with an
  ordered sub-select inside its `GroupBy` — green in tests, untranslatable in SQL Server. It uses plain
  aggregates only; keep it that way.
- **Deferred (designed-for, not built):** supersets/dropsets (reserved `WorkoutSetTypeEnum` slots), rest
  timer, multi-week plans above `WorkoutRoutine`, weekday-scoped supplement slots, per-user hiding of system
  exercises, bodyweight & measurements, progress photos, supplement reminders, and the XP/coin/badge strategy
  behind the hook above.
