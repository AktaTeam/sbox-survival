---
name: commit-and-push
description: Stage, commit with a Conventional Commits message, and push to main on the sbox-survival repo. Use this skill whenever the user says "commit", "push", "save", "commit and push", or whenever a logical chunk of work is complete and ready to be saved (after a feature, fix, refactor, or chore). Also trigger if the user mentions creating a checkpoint or finishing a step.
allowed-tools: Bash(git *)
disable-model-invocation: true
argument-hint: [type] [scope] [description]
---

# Commit and Push

Standardize commit messages and push to main for the sbox-survival project.

## Conventions
- Conventional Commits format: `<type>(<scope>): <short description>`
- Allowed types: feat, fix, chore, refactor, docs, test, perf, style
- Scope is optional but encouraged: `feat(player): ...`, `fix(networking): ...`
- Body explains the *why* and lists what changed (bullet points)
- Breaking changes: add `BREAKING CHANGE:` in footer

## Procedure
1. Run `git status` to see current state
2. If no changes, stop and inform the user
3. Stage changes:
   - If user specified files in arguments, stage those only
   - Otherwise `git add .` (after confirming with user)
4. Build commit message:
   - If $ARGUMENTS provided: type=$1, scope=$2, description=$3+
   - Else: infer from diff and propose for confirmation
5. Show proposed message and ask for explicit confirmation before commit
6. Commit and push to main
7. Report the commit hash

## Safety
- This skill has destructive side effects (push). Always confirm before pushing.
- Never auto-trigger; always require explicit user invocation.
