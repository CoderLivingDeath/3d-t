# AGENTS.md — 3d-t

Unity 6000.4.11f1 · URP 17.4.0 · C# (Assembly-CSharp)

## Stack

| What | Where / How |
|------|-------------|
| **DI** | Reflex 14.3.1 — `MonoBehaviour, IInstaller` → `InstallBindings(ContainerBuilder)` |
| **Async** | UniTask 2.5.11 — use `UniTask` / `UniTask<T>`, never `Task` |
| **Tweening** | LitMotion (UPM git dependency) |
| **Input** | Unity Input System 1.19.0 |

## Project Layout

```
Assets/Project/Scripts/
├── DI/               # Installers — wire everything here
│   ├── ProjectInstaller.cs      # Root scope (NOT IMPLEMENTED)
│   └── GameplayInstaller.cs     # Scene scope (NOT IMPLEMENTED)
├── Features/         # Feature code goes here (currently empty)
├── Infrastructure/   # Core patterns
│   ├── Result.cs / ResultOfT.cs / IResult.cs / ResultExtensions.cs
│   ├── IRepository.cs / IReadOnlyRepository.cs
│   ├── InMemoryRepository.cs
│   ├── ResourceAssetsRepository.cs     # STUB — all methods throw NotImplementedException
│   └── ResourceConfigRepository.cs     # Works — Resources.Load + lazy cache
└── Services/         # App-level services
    ├── IConfigService.cs
    ├── ResourceConfigService.cs
    └── ResourcesAssetsServiceService.cs
```

## Infrastructure Patterns

### Result Pattern (`_3dt.Infrastructure`)
- `IResult` / `IResult<T>` interfaces; `Result` / `Result<T>` are **readonly structs**
- Factory methods:
  - `Result.Ok()`, `Result.Fail(Exception)`, `Result.Fail(string)`
  - `Result<T>.Success(T)`, `Result<T>.Failure(Exception)`, `Result<T>.Failure(string)`
- `IResult<T>` extends `IResult` — both `IsSuccess` + `Error` (Exception) + `GetAllErrors()`
- Extensions in `ResultExtensions`: `Map`, `FlatMap`, `And`, `Or`, `Match`, `IfThen`, `GetErrorMessages`, `GetErrorSummary`
- `ResultTExtensions`: non-boxing overloads directly on `Result<T>` (avoids struct→interface boxing)

### Repository Pattern
- `IRepository<T>` — Add, Get, GetAll, Update, Delete
- `IReadOnlyRepository<T>` — Get, GetAll
- `InMemoryRepository<T>` — Dictionary-backed, ready to use
- `ResourceConfigRepository` — loads via `Resources.Load()`, caches results
- `ResourceAssetsRepository` — **stub**, needs implementation

### Config Service
- `IConfigService.Get<T>(keyOrPath)` returns `IResult<T>`
- `ResourceConfigService` delegates to `ResourceConfigRepository`

## State of the Project (early stage)

| Item | Status |
|------|--------|
| DI installers | ❌ Both throw `NotImplementedException` |
| `ResourceAssetsRepository` | ❌ All methods throw `NotImplementedException` |
| `Features/` | Empty — no feature code yet |
| Tests | None |
| Namespace convention | `_3dt.Infrastructure` for infrastructure; global namespace for installers/services |

## Editor Info

- Reflex root scope: `Assets/Resources/RootScope.prefab`
- Reflex settings: `Assets/Resources/ReflexSettings.asset`
- Config resources (empty): `Assets/Resources/Config/`
- Two URP profiles: `Mobile_RPAsset` / `PC_RPAsset`
- No GitHub Actions, no CI, no pre-commit hooks

## Known pitfalls for agents

1. **Result<T> is a struct** — prefer `ResultTExtensions` over `ResultExtensions` on concrete `Result<T>` to avoid boxing.
2. **Stub files will throw** — `ResourceAssetsRepository` and both installers throw `NotImplementedException`. Any code path hitting them at runtime will crash.
3. **No tests exist** — don't expect a test runner; tests must be created from scratch if needed.
4. **Build in Unity Editor only** — `.csproj` files are auto-generated; don't rely on `dotnet build` for correctness.
