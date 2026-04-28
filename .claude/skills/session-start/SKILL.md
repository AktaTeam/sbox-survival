---
name: session-start
description: Provide a fast project status summary at the start of a Claude Code session. Reads git history, working tree state, last CLAUDE.md updates, and gives a 5-line "where we are" recap. Use this skill whenever the user says "where are we", "status", "recap", "session start", "I'm back", "let's continue", "where did we leave off", or at the very beginning of a new session before doing any work.
allowed-tools: Bash(git *), Read
disable-model-invocation: true
---

# Session Start

Quick orientation summary so we don't waste time figuring out where we left off.

## Procedure
1. Run `git log --oneline -10` to see recent commits
2. Run `git status` to see uncommitted changes
3. Run `git diff --stat HEAD~5..HEAD` to see scope of recent activity
4. Read the last 50 lines of CLAUDE.md
5. Check if any *.scene_c files are untracked or modified (indicates editor activity)
6. Synthesize a 5-line recap:
   - What was the last completed step
   - What's the current uncommitted state
   - What CLAUDE.md says is next
   - Any potential blocker (uncommitted scene, unmerged branch, etc.)
   - Suggested next action

## Output format
Concise. Bullet points or short paragraphs. No deep analysis unless user asks for it.
