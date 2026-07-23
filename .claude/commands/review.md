---
description: Review the current uncommitted changes by dispatching to the backend and/or frontend reviewer subagents based on what changed.
allowed-tools: Bash(git status:*), Bash(git diff:*)
---

Changed files:
!`git status --short`

Full diff (working tree + staged vs HEAD):
!`git diff HEAD`

Orchestrate the review — you are the dispatcher, the reviewers are read-only subagents that
cannot run git, so each one only sees what you put in its prompt:

1. **Scope the diff.** From the changed files above, split the diff into a **backend** slice
   (paths under `src/`), a **frontend** slice (paths under `web/`), and a **tests** slice (paths
   under `tests/`). Ignore tooling-only changes (`.claude/`, config) unless the user asked about
   them.

2. **Dispatch to the specialists that apply** — and when more than one area changed, launch them
   **in parallel** (all Agent calls in a single message):
   - Backend slice non-empty → **backend-reviewer**, passing that slice as the scope.
   - Frontend slice non-empty → **frontend-reviewer**, passing that slice as the scope.
   - Tests slice non-empty → **test-reviewer**, passing that slice as the scope.
   Give each subagent only its own slice so it stays focused; it may read any file for context.

3. **Merge and report.** Combine the findings under one report:

   - **Backend** — *Violations* / *Warnings* (omit this section if no backend changes).
   - **Frontend** — *Violations* / *Warnings* (omit this section if no frontend changes).
   - **Tests** — *Violations* / *Warnings* (omit this section if no test changes).

   Preserve each `file:line`. If a reviewer found nothing, say so for that area. If there are no
   changes at all, say so and don't launch anything.
