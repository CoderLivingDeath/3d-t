# 3d-t

Unity 6000.4.11f1 · URP 17.4.0

## Structure

```
Assets/Project/Scripts/
├── DI/                       # Reflex DI installers
│   ├── ProjectInstaller.cs   # Root-scope bindings
│   └── GameplayInstaller.cs  # Scene-scope bindings
├── Domain/                   # Domain models (ScriptableObject-based)
│   └── Config.cs
├── Infrastructure/           # Core patterns
│   ├── Result / IResult      # Result pattern (struct)
│   ├── IRepository / IReadOnlyRepository
│   ├── InMemoryRepository    # Dictionary-backed repo
│   ├── ResourceConfigRepository   # Resources.Load + cache
│   └── ResourceAssetsRepository   # Stub (TODO)
├── Services/                 # App-level services
│   ├── IConfigService / ResourceConfigService
│   ├── IAssetsService / ResourcesAssetsServiceService
│   ├── IInputService / InputService      # InputSystem_Actions wrapper
│   └── ServicesComposite     # Service registration hub
└── Features/                 # Feature modules
    ├── AFPC/                 # Advanced First-Person Controller
    └── Door/                 # DoorBehaviour
```

## Stack

| Library | Version | Notes |
|---------|---------|-------|
| Reflex  | 14.3.1  | DI: `MonoBehaviour, IInstaller` → `InstallBindings(ContainerBuilder)` |
| UniTask | 2.5.11  | Use `UniTask`/`UniTask<T>`, never `Task` |
| LitMotion | UPM   | Tweening |
| Input System | 1.19.0 | Generated `InputSystem_Actions` |
