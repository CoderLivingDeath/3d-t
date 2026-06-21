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

| Library | Type | Notes |
|---------|------|-------|
| Reflex  | DI | `MonoBehaviour, IInstaller` → `InstallBindings(ContainerBuilder)` |
| UniTask | Async | Use `UniTask`/`UniTask<T>`, never `Task` |
| LitMotion | Tweening | UPM git dependency |
| Unity MCP | AI tooling | Unity → Claude/Copilot bridge |
| NuGetForUnity | Package mgmt | NuGet packages in Unity |
| Cinemachine | Camera | v3.1 |
| ProBuilder | Level design | v6.1 |
| Input System | Input | v1.19 — generated `InputSystem_Actions` |
| URP | Rendering | v17.4 |
| VFX Graph | VFX | v17.4 |
| AFPC | Character | Advanced First-Person Controller (asset) |
