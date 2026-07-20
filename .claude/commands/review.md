---
description: Review the current uncommitted changes with the vacayay-reviewer subagent.
allowed-tools: Bash(git status:*), Bash(git diff:*)
---

Current changes to review:

Status:
!`git status --short`

Diff (working tree + staged vs HEAD):
!`git diff HEAD`

Launch the **vacayay-reviewer** subagent to review the changes shown above against VacaYAY's
architecture and guardrails. The subagent can read any file for context but has no diff access
of its own, so treat the diff above as the scope. Report its findings back grouped as
**Violations** / **Warnings**, each with `file:line`. If there are no changes, say so.
