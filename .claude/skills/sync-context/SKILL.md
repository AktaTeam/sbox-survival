---
name: sync-context
description: Generate a context summary for sharing between Claude Code (local terminal) and Claude chat (web). Reads recent commits, current branch state, modified files, and outputs a copy-paste-ready summary that the user can paste into a chat conversation to keep both AIs in sync. Use this skill whenever the user says "sync with chat", "summary for Claude", "what should I tell Claude", "context for chat", "give me a recap to share", or when they're about to switch from terminal to web chat (or vice versa).
allowed-tools: Bash(git *), Read, Grep
disable-model-invocation: true
---

# Sync Context

Bridge between Claude Code (local) and Claude chat (web). Generate a copy-paste recap.

## Procedure
1. Run `git log --oneline -10`
2. Run `git status`
3. Run `git diff --stat HEAD~3..HEAD` to see what changed recently
4. Identify the current "phase" by reading CLAUDE.md and recent commits
5. List any open TODO comments in modified files (Grep for "TODO" in Code/)
6. Produce a structured markdown recap using this exact template:

---
## Project State Sync - <today's date>

**Current phase**: <inferred from recent commits + CLAUDE.md>

**Last 5 commits**:
- <hash>: <message>
- <hash>: <message>
- <hash>: <message>
- <hash>: <message>
- <hash>: <message>

**Uncommitted changes**:
<git status summary, or "clean working tree" if none>

**Recent activity** (last 3 commits diff stat):
<diff stat summary>

**Open TODOs in modified files**:
- <file>:<line>: <todo text>
(or "none" if no TODOs found)

**My intuition for next step**:
<short 1-2 sentence suggestion based on context>
---

7. After producing the recap, tell the user: "Copy the block above and paste it into Claude chat to brief them on the current project state."

## Safety
- READ-ONLY. Never modify any file.
- Never commit, push, or stage anything.
