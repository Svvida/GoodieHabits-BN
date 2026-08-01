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
`Accounts`, `Auth`, `Badges`, `FriendInvitations`, `Friendships`, `Inventories`, `Leaderboard`, `Nicknames`, `Notifications`, `QuestLabels`, `Quests`, `Shop`, `Statistics`, `UserBlocks`, `UserGoals`, `UserProfiles`, plus shared `Common`.

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
- **Pure calculators** (`Domain/Calculators/`): `NextResetDateCalculator`, `QuestStatisticsCalculator`, `QuestWindowCalculator` — stateless, side-effect-free business math.
- **Domain events** (`Domain/Events/`): e.g. `QuestDeletedEvent`, `BadgeAwardedEvent`.
- **Exceptions** (`Domain/Exceptions/`): all derive from `AppException`, which carries an HTTP `StatusCode`. Examples: `NotFoundException`, `ConflictException`, `ForbiddenException`, `UnauthorizedException`, `InvalidArgumentException`, `PurchaseItemException`, `FriendshipException`. The API middleware relies on this hierarchy.
- **Interfaces** (`Domain/Interfaces/`): repository contracts, `IUnitOfWork`, `ITokenGenerator`/`ITokenValidator`, `INicknameGenerator`. These are implemented in Infrastructure.

### The Quest model (worth understanding)
`Domain/Models/Quest.cs` is a **single entity discriminated by `QuestType`** (Daily/Weekly/Monthly/OneTime/Seasonal) rather than a class hierarchy. Type-specific data lives in **satellite entities**: `MonthlyQuest_Days`, `WeeklyQuest_Day`, `SeasonalQuest_Season`. Repeatable quests (Daily/Weekly/Monthly) also own `QuestOccurrence` records and `QuestStatistics`. The entity encapsulates completion, XP/reward calculation, occurrence generation, reset logic, and statistics recalculation.

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
- **`BackgroundTasks/`** — `IHostedService`s deriving from `StartupTask`. They create a DI scope and dispatch a MediatR command: `ResetQuestsTask`, `ExpireGoalsTask`, `ProcessOccurrencesTask`, `RecalculateRepeatableQuestStatisticsTask`. This keeps scheduled/maintenance work expressed as ordinary application use cases.
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

To add a use case (this is also the path for the upcoming **finance module** — a new feature domain such as `Application/Finance/`):

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
| Finance module (detail) | `FINANCE_MODULE_PLAN.md` (decisions, build log, FE schema) |

---

## 13. Finance module

A personal-finance feature domain (income/expense tracking, hierarchical categories, budgets, analytics) built as
a standard vertical slice under `Application/Finance/`, linked to **`UserProfile`** (not the auth `Account`). It
follows every convention above; the full design rationale, build log, and open questions live in
[`FINANCE_MODULE_PLAN.md`](./FINANCE_MODULE_PLAN.md). Key architectural decisions:

- **Entities** (`Domain/Models/`): `FinanceTransaction` (single entity discriminated by `FinanceTransactionTypeEnum`,
  mirroring `Quest`), `FinanceCategory` (self-referencing, one level deep; system rows seeded via `HasData` with
  `UserProfileId = null`, like `Badge`), `Budget` (overall or per-category, monthly/yearly).
- **Inheritance rule for categories:** a sub-category derives both `Type` and `IsSavings` from its parent — the
  values sent for a sub are ignored, and changing `IsSavings` on a main cascades to its subs. `IsSavings` is a
  descriptive tag only: analytics deliberately treat savings transactions like any other.
- **Money & time:** amounts are `decimal(18,2)`, always positive (sign implied by type). A transaction's
  `OccurredOn` is a **`DateOnly`** (SQL `date`) — a calendar fact, deliberately *not* a UTC instant — which
  sidesteps timezone/month-boundary bugs in analytics. Audit timestamps stay UTC. Currency is a single ISO-4217
  string per user on `UserProfile.Currency` (allow-list in `Domain/ValueObjects/SupportedCurrencies.cs`).
- **Calculators** (`Domain/Calculators/`): `FinancePeriodCalculator` (timezone-free `DateOnly` bounds) and
  `BudgetProgressCalculator` (spent-vs-limit) — pure, DB-free, unit-tested.
- **Slices:** `Application/Finance/{Categories,Transactions,Budgets,Analytics,Settings}/`. Relational checks
  (ownership, category/type consistency, uniqueness) live in handlers (→ `NotFound`/`Conflict`); validators cover
  field rules. Analytics queries aggregate in-memory over `IFinanceTransactionRepository.GetForPeriodAsync`.
- **API:** thin controllers under `/api/finance/{categories,transactions,budgets,analytics,settings}`. No
  `Program.cs` changes were needed — handlers/validators/Mapster profiles are picked up by the existing assembly
  scans, and the three new repositories are exposed through `IUnitOfWork`.
- **FE contract:** hand-written TypeScript types in `docs/finance-api-schema.ts` (enums serialize as strings,
  `DateOnly` → `"YYYY-MM-DD"`), alongside the generated `docs/swagger.json`.
- **Corrections** (specced, not built — Phase 11): refunds/paybacks/reimbursements are modelled as a *relation*
  between transactions (`CorrectsTransactionId` self-FK), not as a transaction type — the reserved
  `FinanceTransactionTypeEnum.Transfer` slot stays reserved for wallets. A correction inherits its parent's
  `Type` and `CategoryId`; netting is materialized on the parent (`NetAmount = Amount - CorrectedAmount`) so the
  analytics handlers only swap which property they sum. Rationale in `FINANCE_MODULE_PLAN.md` §11.
- **Deferred (designed-for, not built):** recurring transactions (background task), multiple wallets + transfers,
  multi-currency (`Money` VO + FX), gamification hooks, receipt attachments.
