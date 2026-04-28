---
name: move-asset
description: Move an s&box asset (.scene, .prefab, .vmdl, .vmat) from one folder to another, handling compiled artifacts (.scene_c, .scene_d, .vmdl_c, .vmat_c) and updating references in sbox-survival.sbproj and CLAUDE.md. Use this skill whenever the user wants to move, relocate, reorganize, or restructure an s&box asset, including scene files, prefabs, models, materials, or any file with a compiled .scene_c/.vmdl_c/.vmat_c counterpart. Also trigger on words like "move", "relocate", "reorganize", "restructure", "fix path".
allowed-tools: Bash(git *), Bash(mv *), Bash(rm *), Read, Edit, Glob, Grep
disable-model-invocation: true
argument-hint: [filename] [from-folder] [to-folder]
---

# Move Asset

Safely move s&box assets including compiled artifacts and reference updates.

## Critical preconditions
- s&box editor MUST be closed (file references corrupt otherwise)
- ALWAYS verify with the user that s&box is closed before any file operation
- If unsure, ask explicitly

## Procedure
1. Confirm editor closed with the user
2. Identify the asset and its compiled artifacts:
   - .scene → also handles .scene_c and .scene_d
   - .vmdl → also handles .vmdl_c
   - .vmat → also handles .vmat_c
   - .prefab → single file, no compiled artifact
3. Move main file + all compiled artifacts to destination folder
4. Update sbox-survival.sbproj if it references the old path:
   - Look for: StartupScene, MapStartupScene, DefaultScene, MapList
5. Update CLAUDE.md if old path is referenced
6. Search Code/**/*.cs for hardcoded path references and flag to user
7. Verify .gitignore handles compiled artifacts (*.scene_c, *.vmdl_c should be gitignored)
8. Stage all changes, propose commit message:
   `fix(assets): move <filename> from <from-folder> to <to-folder>`
9. Ask user confirmation before commit + push

## Safety
- Destructive operation: always require explicit user invocation
- Always confirm s&box editor is closed before moving files
- Never delete the source file before confirming the destination move succeeded
