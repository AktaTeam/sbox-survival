---
name: add-component
description: Generate a new s&box C# Component with project conventions (namespace SboxSurvival, Component inheritance, [Property] attributes, [Sync] if networked, lifecycle methods, doc XML). Use this skill whenever the user wants to create, add, scaffold, or generate a new component, system, or game logic class for the sbox-survival project. Trigger on phrases like "add a component", "create a component", "new component", "scaffold a class", "generate", "I need a component for".
allowed-tools: Read, Write, Edit
disable-model-invocation: true
argument-hint: [component-name] [folder] [networked?]
---

# Add Component

Scaffold a new s&box Component following sbox-survival conventions.

## Conventions enforced (per CLAUDE.md)
- Namespace: SboxSurvival (file-scoped)
- Inherit from `Sandbox.Component`
- Editor-tunable: `[Property]` (with `[Range(...)]` when bounded)
- Networked: `[Sync]` for synced properties
- Runtime-only state: `{ get; private set; }` without `[Property]`
- `OnFixedUpdate` for physics/sim, `OnUpdate` for input/camera/timers
- Sibling refs via `[Property]` with `??= GetComponent<T>()` fallback in `OnStart`
- Avoid `Scene.GetAllComponents` for player-local systems

## Procedure
1. Parse $ARGUMENTS:
   - $1 = component name (PascalCase, e.g. "Equipment", "DayNightCycle")
   - $2 = target folder relative to Code/ (e.g. "Player", "World", "Systems")
   - $3 (optional) = "networked" if component holds shared state
2. Check target folder exists, create if needed
3. Generate Code/$2/$1.cs:
   - `using` directives
   - File-scoped namespace `SboxSurvival`
   - `public sealed class $1 : Component`
   - XML doc comment summarizing purpose (ask user to confirm summary)
   - Lifecycle stubs: OnAwake, OnStart, OnUpdate (commented appropriately)
   - If networked: `[Sync]` examples + `IsProxy` guards in mutators
   - TODOs for actual logic
4. Update CLAUDE.md "Composition" section if component is core to player or world
5. Show generated file, ask for confirmation
6. Do NOT auto-commit - user runs commit-and-push skill separately

## Safety
- File creation: always require explicit user invocation
- Never overwrite existing files without confirmation
