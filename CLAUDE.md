# CLAUDE.md

Guidance for AI agents (and humans) working in this repository. **Read [`ARCHITECTURE.md`](./ARCHITECTURE.md) first** — it is the full knowledge base (layers, patterns, file map, how to add a feature). This file is the short list of hard rules.

## What this is
Backend REST API (.NET 8, ASP.NET Core) for a gamified habit/quest-tracking mobile app. Clean/Onion + Vertical Slice architecture. Projects: **Domain → Application → Infrastructure → Api** (+ `Application.Tests`).

## Golden rules
- **Dependencies point inward.** Domain depends on nothing (except NodaTime); Application depends on Domain; Infrastructure/Api implement inward-defined interfaces. Never make Domain or Application reference Infrastructure — define an interface inward and wire it in `Api/Program.cs`.
- **No business logic in controllers.** Controllers map `Request → Command/Query`, attach identity via `User.GetCurrentUserProfileId()`, `sender.Send(...)`, and return. Logic lives in Application handlers and Domain models.
- **CQRS via MediatR.** Every use case is an `ICommand`/`IQuery` + handler. Validation runs automatically through `ValidationBehavior` — add a FluentValidation `XxxCommandValidator` per command.
- **Rich domain models.** Construct entities via `static Create(...)` factories; keep setters `private`; change state through intention-revealing methods that enforce invariants. Throw `AppException` subclasses (they carry HTTP status codes) for error paths.
- **Persistence through `IUnitOfWork`.** Handlers depend on `IUnitOfWork`, not individual repositories. One `SaveChangesAsync` per unit of work. Schema changes require an EF Core migration.
- **Mapping = Mapster** (`IMapper`, profiles implement `IRegister`). Not AutoMapper.
- **Time = NodaTime.** Inject `IClock`; avoid `DateTime.Now`/`SystemClock.Instance` directly.

## Conventions
- Block-scoped namespaces; primary constructors; `record` commands/queries.
- Naming: `Xxx{Command,Query,Handler,Validator,Request,Response,Dto,MappingProfile}`.
- Slice layout: `Application/<Feature>/Commands|Queries/<UseCase>/…` (see ARCHITECTURE.md §5).
- Adding a feature: follow the recipe in ARCHITECTURE.md §11 (Domain → Persistence + migration → Application slice → thin controller + DI in `Program.cs` → tests).

## Tests
`Application.Tests` (xUnit + Moq + FluentAssertions, EF Core InMemory) is **mid-refactor** — see ARCHITECTURE.md §10. Prefer builders/factories over reflection for test data; note handlers are invoked directly, so `ValidationBehavior` is not exercised (validators need their own tests).

## Build / test
```
dotnet build GoodieHabits.sln
dotnet test Application.Tests
```
