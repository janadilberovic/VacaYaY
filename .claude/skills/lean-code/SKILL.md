---
name: lean-code
description: Write and edit code without unnecessary comments or superfluous code. Use whenever writing new code, editing existing code, or reviewing a diff in VacaYAY — especially when tempted to add explanatory comments, defensive scaffolding, or restate what the code already says. Apply on every "add", "implement", "write", "fix", or "refactor" task, not only when the user explicitly mentions comments.
---

# Lean code

Good code in this repo reads cleanly on its own. Comments and extra code are a cost — they
have to be kept true as the code changes, and stale or noisy ones actively mislead the next
reader. Add them only when they earn their place. When in doubt, leave it out.

The goal isn't zero comments — it's zero *unnecessary* ones. The same applies to code: write
what the task needs, nothing speculative.

## Comments: what to cut

Delete or never write comments that just restate the code. The reader can see what the line
does; a comment that echoes it adds noise and one more thing to keep in sync.

**Cut these:**

```csharp
// Get the user by id                          ← the method name already says this
var user = await _repo.GetByIdAsync(id, ct);

// Loop through requests                        ← obvious from the foreach
foreach (var request in requests) { ... }

// Constructor                                  ← it's visibly a constructor
public AuthService(VacaYAYDbContext db) { ... }

// Return the DTO
return user.Adapt<UserResponse>();              ← restates the return
```

Also cut: commented-out code (git remembers it), `// TODO` notes with no plan behind them,
changelog comments (`// added by X on date`), and section-divider banners in small files.

## Comments: what to keep

Keep a comment when it carries information the code *cannot*: why a non-obvious choice was
made, a subtle constraint, a workaround for an external quirk, or a warning about a sharp edge.
This repo already models the good kind — match it:

```csharp
// first login uses plaintext TempPassword compared with FixedTimeEquals (timing-safe)
// TokenDenylist is singleton — in-memory, per-process only; flagged for Redis if scaled out
// enums persisted as strings so the DB stays readable
```

The test: if the comment explains **why** and would be hard to reconstruct from the code alone,
keep it. If it explains **what** and the code already shows that, cut it.

XML doc comments (`/// <summary>`) are not required here — add them only where a public API's
intent is genuinely non-obvious, not as boilerplate on every member.

## Code: what to cut

Don't write code the task didn't ask for. Speculative generality is the same kind of debt as a
stale comment — it has to be read, understood, and maintained, and it usually guesses wrong.

- **No speculative abstraction** — don't add an interface, generic, or config hook for a second
  use case that doesn't exist yet. Add it when the second case arrives.
- **No dead parameters or options** — flags nothing sets, overloads nothing calls.
- **No redundant guards** — one null-check, not the same check at three layers. Trust the
  boundary that already validates (controllers validate via FluentValidation; services can
  assume validated input).
- **No reinventing what exists** — reuse the repo's helpers (`ClaimsPrincipalExtensions`,
  Mapster `.Adapt<T>()`, existing validators) instead of hand-rolling equivalents.
- **Don't restate defaults** — e.g. `CancellationToken cancellationToken = default` is the
  convention; don't wrap every call in extra ceremony around it.

## When editing existing code

Leave it cleaner than you found it, but stay in scope: if you touch a block that has a
redundant comment or dead line, drop it. Don't launch a repo-wide comment purge unless asked —
and never delete a comment you don't understand. A comment you can't explain may be load-bearing
(the "why" just isn't obvious yet); preserve it or ask.

## The habit

Before finishing, reread the diff and ask of each comment: *does this tell the reader something
the code can't?* And of each block: *did the task actually need this?* Cut what fails. A smaller,
quieter diff is the goal.
