# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Single-player s&box (Facepunch's Source 2 sandbox platform) survival game. C# game logic only — no standalone toolchain. Building, running, and asset compilation all happen through the s&box editor.

The s&box API moves quickly; verify attribute names and `Component` lifecycle method signatures against the installed editor version before assuming an API is current.

## Build / Run / Iterate

There is no `dotnet build` or test runner here — s&box compiles `Code/` itself.

- Open the project: launch s&box, then **Open Project → `sbox-survival.sbproj`**.
- Compile after edits: Ctrl+B in the s&box editor (or save a `.cs` file — hot reload picks it up).
- Run: open `Assets/Scenes/main.scene` (referenced as `StartupScene` in the .sbproj) and press Play.
- Edit `GameResource` instances (e.g. `ItemDefinition` `.item` files) through the editor's resource inspector, not by hand.

## Architecture

Scene + Component model (similar to Unity), not the legacy s&box Entity system. Game state lives on `GameObject`s in the scene; logic lives on `Component`s attached to them. Use `[Property]` to expose fields to the editor.

### Composition of the player

The player is **multiple components on one GameObject**, deliberately decomposed so each system is testable and replaceable:

- `PlayerController` (native s&box component) — input sampling, camera look, locomotion, jump, run. No project-side code; configured via inspector tunables on the player GameObject.
- `SprintStaminaController` (`Code/Player/SprintStaminaController.cs`) — Bridges PlayerController (native) to SurvivalStats via PlayerController.IEvents. Drains stamina on sprint and jump.
- `SurvivalStats` (`Code/Player/SurvivalStats.cs`) — single source of truth for health, hunger, thirst, stamina. Exposes `Eat/Drink/Heal/ApplyDamage/CanAffordStamina/DrainStamina` so other systems mutate stats only through this surface (host-authoritative via [Rpc.Host] for mutators). Drains hunger/thirst on `OnFixedUpdate` and applies starvation damage when either reaches zero.
- `Inventory` (`Code/Player/Inventory.cs`) — fixed-slot stack-based inventory keyed on `ItemDefinition`. `TryAdd` fills existing stacks first, then empty slots; returns `false` if items couldn't all fit.
- `Interactor` (`Code/World/Interactor.cs`) — eye-ray tracer that translates `attack1` into harvest calls on whatever `ResourceNode` it hits. The interactor holds the equipped `ToolKind` and swing damage; the player GameObject does not interact with the world directly through other components.

When adding a new player capability (e.g. crafting, equipment), prefer a new `Component` over expanding existing ones (composition over inheritance). Wire it up through `[Property]` references rather than `Scene.GetAllComponents<...>()` lookups.

### World objects

`ResourceNode` (`Code/World/ResourceNode.cs`) is the model for any harvestable in the world (trees, rocks, bushes). It owns its own HP, required tool, yield, and respawn timer; it disables its `GameObject` while depleted instead of destroying it. Mirror this pattern for new harvestables — don't add another harvest path on the player or interactor.

### Items as resources

Items are **GameResources**, not C# classes per item. `ItemDefinition` (`Code/Systems/ItemDefinition.cs`) carries `[GameResource]`, so each item is a `.item` file under `Assets/`. `Inventory` and `ResourceNode` reference items by `ItemDefinition` instance — adding a new item is an editor-side action (create `.item` resource), not a code change.

To add a new item *category* or *behavior* (e.g. armor stats, durability), extend `ItemDefinition` with new fields and consume them from the system that cares (`SurvivalStats` for consumables, a future `Equipment` component for tools).

### Game lifecycle

`GameManager` (`Code/GameManager.cs`) is a scene-singleton (`GameManager.Current`). It owns:
- Player spawning from `PlayerPrefab` at `SpawnPoint` on `OnStart`.
- Day/night clock (`TimeOfDay`, `IsNight`) advanced in `OnUpdate`. Other systems should *read* `GameManager.Current.TimeOfDay` rather than running their own clocks.

### Networking

The project is **multiplayer** (`Metadata.GameNetworkType: Multiplayer`, `MaxPlayers: 16`, `LaunchMode: QuickPlay`). Components must be designed network-aware from the start:

- Synchronized state uses `[Sync]` on properties that all clients need to observe (health, position, equipped tool, etc.).
- Distinguish ownership with `IsProxy` — the owning client/host returns `false`; remote replicas return `true`. Input sampling, authoritative simulation, and one-shot side effects must be gated on `!IsProxy`.
- RPC routing: `[Rpc.Broadcast]` to fan out to everyone, `[Rpc.Host]` to send a request to the host (authoritative writes), `[Rpc.Owner]` to target the object's owner (e.g. local-only feedback).

**Engine limits to know before scaling up:**
- `MaxPlayers` is capped at 64 for stable play on premium hardware (256 is the theoretical engine ceiling but unstable in practice). 16 is our prototype target — adjust upward only after profiling.
- World bounds are ±32768 units; lighting bake is unavailable beyond that.
- 1 unit ≈ 2.54 cm — keep movement speeds, hitboxes, and ranges in source units.

**Phase C — networking refactor status:**
- ✅ `SurvivalStats` — refactored to multiplayer (commit 331b18b: [Sync] state, [Rpc.Host] mutators with caller authorization).
- ✅ `Player` (legacy) — replaced by `SprintStaminaController` (commits c63f52d / 2fd64d0). PlayerController native handles movement/camera/input; SprintStaminaController bridges to SurvivalStats via IEvents.
- 🟡 `Inventory` — not yet audited. Needs network adaptation before features depending on it (item drops, container interactions) are built.
- 🟡 `Interactor` — not yet audited. Needs Client → Rpc.Host pattern for harvest calls, plus `ResourceNode` host-authoritative damage.
- 🟡 `ResourceNode` (`Code/World/`) — not yet audited. Single-source-of-truth health on host, damage RPCs from any client tool swing.

## Conventions specific to this codebase

- Namespace is `SboxSurvival` (set in `sbox-survival.sbproj` `RootNamespace`); every file uses `namespace SboxSurvival;` (file-scoped).
- Editor-tunable values use `[Property, Range(...)]`; runtime state uses `{ get; private set; }`. If a value should *not* be editable in-editor, leave the `[Property]` off.
- Mutate `SurvivalStats` only through its public methods. Direct field assignment defeats clamping and the `OnDied` event.
- `OnFixedUpdate` for physics/simulation drains and movement; `OnUpdate` for input, camera, and timers. Don't sample `Input` from `OnFixedUpdate`.
- Components find their siblings via `[Property]` references wired in the editor, with a `??= GetComponent<T>()` fallback in `OnStart` for resilience. Avoid `Scene.GetAllComponents` for player-local systems.

## Available Skills

Project-scoped Claude Code skills in `.claude/skills/`:

- **commit-and-push** (manual): Stage, Conventional Commits message, push to main.
- **move-asset** (manual): Move s&box assets safely with compiled artifacts and reference updates.
- **audit-network** (auto): Read-only audit of C# components for multiplayer compliance.
- **add-component** (manual): Scaffold a new s&box Component with project conventions.
- **session-start** (manual): Quick recap of project state at session start.
- **sync-context** (manual): Generate a copy-paste summary to brief Claude chat.
- **fix-from-error** (auto): Diagnose s&box errors and propose patches.

Manual skills require explicit user invocation (`/skill-name`). Auto skills can trigger on context match.
