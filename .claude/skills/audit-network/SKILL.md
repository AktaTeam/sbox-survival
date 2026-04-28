---
name: audit-network
description: Audit a C# Component file or folder for s&box multiplayer correctness. Verifies proper [Sync] attribute usage, IsProxy guards, RPC attributes (Broadcast/Host/Owner), and authority placement. Use this skill whenever the user asks to review networking code, check multiplayer compliance, audit a component, find sync issues, or whenever you (Claude) detect a C# component file in the sbox-survival project that holds shared state. Also trigger on phrases like "audit network", "check sync", "review multiplayer", "is this networked correctly", "any [Sync] missing", "ready for multi".
allowed-tools: Read, Glob, Grep
argument-hint: [file-or-folder-path]
---

# Audit Network Compliance

Read-only audit of s&box C# components for multiplayer correctness.

## Project context
- GameNetworkType: Multiplayer (8 max players, coop survival)
- Components live in Code/Player, Code/World, Code/Systems
- Reference patterns from official Facepunch examples (sbox-hc1, sbox-pool2, sbox-scenestaging)

## Reference patterns to check against
- Properties holding shared state need [Sync]
- State-mutating logic guarded by `if ( IsProxy ) return;`
- Cross-client procedural calls use [Rpc.Broadcast], [Rpc.Host], or [Rpc.Owner]
- INetworkListener interface for connection lifecycle
- NetList<T> and NetDictionary<K,V> for synchronized collections
- Network.Refresh() to force sync after batch state changes
- Avoid legacy [Net] attribute (old Entity system)

## Audit checklist (per file)
1. List all public properties with setters → flag any holding shared state but lacking [Sync]
2. List all methods that mutate state → flag any lacking `if ( IsProxy ) return;` guard at entry (when appropriate)
3. List all RPC-style methods → check for proper RPC attribute
4. Identify state ownership: server-authoritative, client-authoritative, shared
5. Check for legacy s&box networking API
6. Cross-reference against CLAUDE.md project conventions

## Output format
- Per-file report with severity: 🔴 critical, 🟡 warning, 🟢 ok
- Concrete fix suggestions for each finding
- Order recommended fixes (critical first)

## Safety
- READ-ONLY. Never modify files. Always suggest fixes for user approval.
