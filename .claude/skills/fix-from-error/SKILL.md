---
name: fix-from-error
description: Diagnose and propose a fix for a C# compilation error or runtime error from the s&box console. Identifies the file, line, root cause, and suggests a patch. Use this skill whenever the user pastes an error message, stack trace, exception, compilation error, runtime crash, ERROR_FILEOPEN, NullReferenceException, or any text that looks like an error from the s&box editor console or terminal. Also trigger on phrases like "this error", "what does this mean", "fix this", "why is this broken", "compile error".
allowed-tools: Read, Grep, Glob, Edit
---

# Fix From Error

Diagnose error messages and propose targeted fixes.

## Procedure
1. Parse the error message:
   - File path (if mentioned)
   - Line number (if mentioned)
   - Error type (compile, runtime, asset)
   - Error code (CS####, ERROR_FILEOPEN, etc.)
2. If file path is mentioned, Read the file at that location
3. Use Grep to find related context (e.g., where the symbol is defined, where it's used)
4. Identify root cause:
   - Syntax error → fix syntax
   - Missing reference → suggest import or reference fix
   - API change (s&box version drift) → flag for verification against current Sandbox.* API
   - Logic error → explain what's happening and suggest correction
5. Propose a concrete patch (diff format) for user approval
6. If error is benign (e.g. ERROR_FILEOPEN on default assets), tell user to ignore it

## Common s&box error patterns
- `ERROR_FILEOPEN: File not found - sounds/...` → benign, default assets, ignore
- `ERROR_FILEOPEN: File not found - models/citizen_props/...` → benign, default assets
- `CS0246: type or namespace name not found` → missing using directive or wrong namespace
- `CS0103: name does not exist in current context` → typo or missing reference
- `Sandbox.X is obsolete` → API drift, suggest migration to current API
- `NullReferenceException` in OnStart → likely missing [Property] reference or GetComponent fallback
- `[Sync]` property not syncing across clients → check IsProxy guards and that property has a setter
- French decimal separator in numeric input → suggest using `.` not `,`

## Output format
- Brief diagnosis (1-2 sentences)
- Root cause explanation
- Concrete fix as a diff or code block
- Apply via Edit tool ONLY after user confirms

## Safety
- Suggest fixes via Edit tool but always show the diff and ask before applying
- Never apply structural changes (file move, refactor) without explicit user approval
- For obsolete API errors, recommend the user verify with chat-Claude (who has Context7 access to live docs) before patching blindly
