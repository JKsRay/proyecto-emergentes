# AGENTS.md — proyecto-emergentes

## Project identity

Unity 6 (6000.4.0f1) AR mobile game — virtual pet that talks via a local LLM.
Target: **Android** (ARCore), URP rendering, IL2CPP scripting backend.

Main scene: `Assets/AR_MainScene.unity`

## Before you touch anything

### Unity-specific constraints
- **Never rename/move files outside the Unity Editor.** Files have paired `.meta` files with GUIDs. Moving them in Explorer/terminal breaks all references.
- `.csproj` files are gitignored and auto-regenerated. Do not edit them manually.
- **Do NOT manually edit `.prefab`, `.unity`, `.asset`, or `.meta` files.** Unity serializes them as YAML with internal references.

### Build target
- The editor opens in Windows Standalone by default. You **must** switch platform to Android: `File → Build Profiles → Android → Switch Platform`.
- First IL2CPP build takes 5–15 min. Subsequent builds are faster.

### LLM model files
- LLM `.gguf` model files in `Assets/StreamingAssets/` are **gitignored** (several GB each). They must be placed manually.
- `Assets/StreamingAssets/LlamaLib-v2.0.5/` (native DLLs) is also gitignored.

## Architecture

### Layers (top-down ownership)
| Layer | What lives there | Key files |
|---|---|---|
| Core / FSM | Pet state machine, stats, context injection | `_MascotaVirtual/_Scripts/RobotStateManager.cs` |
| LLM | Chat interface, prompt routing | `LLM Manager/` (LLMUnity-based), `ChatController` |
| AR Minigame | AR plane detection, touch-to-catch, nav | `MinigameManager.cs`, `RobotARNavigator.cs`, `TouchCatcher.cs` |
| UI | Canvas, buttons, HUD counters | `_MascotaVirtual/_Scripts/RobotUIManager.cs`, `Canvas.prefab` |
| AR Placement | Initial object placement on detected plane | `Scripts/PlaceObjectOnPlane.cs` |

### Event-driven coupling (DO NOT bypass)
AR module and FSM module **never** reference each other directly. They communicate via static C# `Action` events declared in `MinigameManager`:

```
OnGameStarted → OnRobotCaught → OnGameWon → OnGameEnded
```

- `RobotStateManager` subscribes/unsubscribes in `OnEnable()`/`OnDisable()`.
- If you need cross-module communication, add an event to `MinigameManager` — do NOT add direct script references.

### Singleton
`RobotStateManager.Instance` is set at runtime in `Awake()`. The robot prefab is **instantiated dynamically** by AR Foundation, so `Instance` is null until AR detects a plane. Code that needs the robot must check for null.

## Core game rules (source of truth: `Docs/DOCUMENTACION_FSM.md`)

### FSM States (priority-ordered in `CurrentState` getter)
| Priority | State | Threshold |
|---|---|---|
| 1 | `BateriaCritica` | Energía ≤ 30 |
| 2 | `Descalibrado` | Mantenimiento ≤ 30 |
| 3 | `Aburrido` | Felicidad ≤ 30 |
| 4 | `Euforico` | Felicidad ≥ 80 |
| — | `Normal` | Default |

> **Code is authoritative:** `Docs/DOCUMENTACION_FSM.md` says thresholds are ≤ 20, but the actual `RobotStateManager.CurrentState` getter uses ≤ 30. Trust the code. If you change thresholds, update both the code and that doc.

### Passive decay (coroutines, NOT Update())
- Energía: −2 every 20s
- Felicidad: −2 every 10s
- Mantenimiento: no passive decay (only degrades via actions)
- Decay is **paused** while `isGameActive == true`

### Action costs
| Action | Felicidad | Energía | Mantenimiento |
|---|---|---|---|
| Botón "Jugar" | +30 | −15 | −30 |
| Ganar minijuego | +50 | −15 | −30 |
| Recargar | — | →100 | — |
| Mantenimiento | — | — | →100 |

## Docs

Primary technical documentation is in `Docs/` (Spanish):
- `DOCUMENTACION_ARQUITECTURA.md` — architecture decisions, refactors, runtime patterns
- `DOCUMENTACION_FSM.md` — exact FSM thresholds, costs, coroutine logic, policy for keeping this doc updated when values change
- `Documentacion_MiniJuego.md` — AR minigame architecture, bugfix history, event flow
- `SETUP_ENTORNO_AR.md` — dev environment setup and first build instructions

`AI_FineTuning/` contains dataset generation and fine-tuning pipelines for the LLM persona.

## Key gotchas

- **UI passthrough bug (fixed, don't regress):** Touch input for AR raycasting must check `IsTouchOverUI()` before processing, or UI button taps will also trigger AR placement/grabbing. The fix uses `EventSystem.current.RaycastAll()`.
- **PlanoInvisible_Prefab:** Custom AR plane prefab with MeshRenderer and LineRenderer stripped. Do NOT re-add renderers — the invisible planes preserve collision-only performance.
- **Animation conflicts at game start:** `RobotARNavigator` calls `animator.Rebind()` and waits 1 frame (`yield return null`) before movement to force the robot into idle and suppress dancing animations.
- **Placement lock:** After the robot is instantiated, `_isPlacementLocked = true` prevents teleporting on every touch. There is a public `UnlockPlacement()` API if repositioning is needed.
- **Inspector listeners + code listeners coexist:** `RobotUIManager` injects `onClick.AddListener()` in C#. Do NOT overwrite or remove inspector-assigned UnityEvents — Unity supports both simultaneously, and this avoids merge conflicts on the Canvas prefab.

## Testing

There is **no custom test suite** for game scripts. Editor tests exist only for the LLMUnity package (`Assets/LLMUnity/Tests/Editor/`). Run them from Unity's Test Runner window.

## Relevant files for common tasks

| Task | Files to read |
|---|---|
| Change pet behavior / stats | `RobotStateManager.cs`, `Docs/DOCUMENTACION_FSM.md` |
| Modify AR minigame logic | `MinigameManager.cs`, `RobotARNavigator.cs`, `TouchCatcher.cs`, `Docs/Documentacion_MiniJuego.md` |
| Change UI / buttons | `RobotUIManager.cs`, `Canvas.prefab` |
| Change LLM persona / prompts | `RobotStateManager.cs` (`CurrentState` switch, `CrearContexto()`), `ChatController` |
| Change AR plane / placement | `PlaceObjectOnPlane.cs`, `PlanoInvisible_Prefab.prefab` |
| Add a new scene | Copia `AR_MainScene.unity` desde el Editor, registrá en Build Settings |
| Change Unity version or packages | `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` |
