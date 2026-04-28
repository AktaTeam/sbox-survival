# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Single-player s&box (Facepunch's Source 2 sandbox platform) survival game. C# game logic only — no standalone toolchain. Building, running, and asset compilation all happen through the s&box editor.

The s&box API moves quickly; verify attribute names and `Component` lifecycle method signatures against the installed editor version before assuming an API is current.

## Build / Run / Iterate

There is no `dotnet build` or test runner here — s&box compiles `Code/` itself.

- Open the project: launch s&box, then **Open Project → `sbox-survival.sbproj`**.
- Compile after edits: Ctrl+B in the s&box editor (or save a `.cs` file — hot reload picks it up).
- Run: open `Scenes/main.scene` (referenced as `DefaultScene` in the .sbproj — create it in-editor; it is not in the repo) and press Play.
- Edit `GameResource` instances (e.g. `ItemDefinition` `.item` files) through the editor's resource inspector, not by hand.

## Architecture

Scene + Component model (similar to Unity), not the legacy s&box Entity system. Game state lives on `GameObject`s in the scene; logic lives on `Component`s attached to them. Use `[Property]` to expose fields to the editor.

### Composition of the player

The player is **multiple components on one GameObject**, deliberately decomposed so each system is testable and replaceable:

- `Player` (`Code/Player/Player.cs`) — input, camera look, locomotion. Asks `SurvivalStats` whether sprinting/jumping is allowed and reports stamina cost back. Does **not** own health or hunger.
- `SurvivalStats` (`Code/Player/SurvivalStats.cs`) — single source of truth for health, hunger, thirst, stamina. Exposes `Eat/Drink/Heal/ApplyDamage/TrySpendStamina` so other systems mutate stats only through this surface. Drains hunger/thirst on `OnFixedUpdate` and applies starvation damage when either reaches zero.
- `Inventory` (`Code/Player/Inventory.cs`) — fixed-slot stack-based inventory keyed on `ItemDefinition`. `TryAdd` fills existing stacks first, then empty slots; returns `false` if items couldn't all fit.
- `Interactor` (`Code/World/Interactor.cs`) — eye-ray tracer that translates `attack1` into harvest calls on whatever `ResourceNode` it hits. The interactor holds the equipped `ToolKind` and swing damage; `Player` does not interact with the world directly.

When adding a new player capability (e.g. crafting, equipment), prefer a new `Component` over expanding `Player`. Wire it up through `[Property]` references rather than `Scene.GetAllComponents<...>()` lookups.

### World objects

`ResourceNode` (`Code/World/ResourceNode.cs`) is the model for any harvestable in the world (trees, rocks, bushes). It owns its own HP, required tool, yield, and respawn timer; it disables its `GameObject` while depleted instead of destroying it. Mirror this pattern for new harvestables — don't add another harvest path on the player or interactor.

### Items as resources

Items are **GameResources**, not C# classes per item. `ItemDefinition` (`Code/Systems/ItemDefinition.cs`) carries `[GameResource]`, so each item is a `.item` file under `Assets/`. `Inventory` and `ResourceNode` reference items by `ItemDefinition` instance — adding a new item is an editor-side action (create `.item` resource), not a code change.

To add a new item *category* or *behavior* (e.g. armor stats, durability), extend `ItemDefinition` with new fields and consume them from the system that cares (`SurvivalStats` for consumables, a future `Equipment` component for tools).

### Game lifecycle

`GameManager` (`Code/GameManager.cs`) is a scene-singleton (`GameManager.Current`). It owns:
- Player spawning from `PlayerPrefab` at `SpawnPoint` on `OnStart`.
- Day/night clock (`TimeOfDay`, `IsNight`) advanced in `OnUpdate`. Other systems should *read* `GameManager.Current.TimeOfDay` rather than running their own clocks.

There is no networking layer — `Metadata.GameNetworkType` in the .sbproj is `Singleplayer`. Don't add `[Sync]` / RPC plumbing without changing that first.

## Conventions specific to this codebase

- Namespace is `SboxSurvival` (set in `sbox-survival.sbproj` `RootNamespace`); every file uses `namespace SboxSurvival;` (file-scoped).
- Editor-tunable values use `[Property, Range(...)]`; runtime state uses `{ get; private set; }`. If a value should *not* be editable in-editor, leave the `[Property]` off.
- Mutate `SurvivalStats` only through its public methods. Direct field assignment defeats clamping and the `OnDied` event.
- `OnFixedUpdate` for physics/simulation drains and movement; `OnUpdate` for input, camera, and timers. Don't sample `Input` from `OnFixedUpdate`.
- Components find their siblings via `[Property]` references wired in the editor, with a `??= GetComponent<T>()` fallback in `OnStart` for resilience. Avoid `Scene.GetAllComponents` for player-local systems.
