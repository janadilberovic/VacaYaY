---
description: Branch, commit, push, and open a pull request for the current changes with the pr-author subagent.
argument-hint: <optional title or focus for the PR>
allowed-tools: Bash(git status:*), Bash(git diff:*), Bash(git log:*), Bash(git branch:*), Bash(git remote:*)
---

Current branch:
!`git branch --show-current`

Remote:
!`git remote -v`

Changed files:
!`git status --short`

Commits not on main:
!`git log --oneline main..HEAD`

Full diff (working tree + staged vs HEAD):
!`git diff HEAD`

Launch the **pr-author** subagent to open a pull request for the above, passing it all of this
context plus any extra focus the user gave: $ARGUMENTS

The subagent branches, commits, pushes, and creates the PR, but it stops at two checkpoints — once
before committing and once before pushing. Relay each checkpoint to me and wait for my answer
before letting it continue; do not approve on my behalf. If there is nothing to commit and nothing
unpushed, say so and don't launch anything.
