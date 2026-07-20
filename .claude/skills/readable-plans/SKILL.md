---
name: readable-plans
description: Present implementation plans as short, feature-grouped sections walked through one at a time, in plain language the user can follow. Use whenever you are about to propose a plan, outline an approach, describe how you'd implement something, or enter plan mode in VacaYAY — even when the user just asks "how would you do this?". Prefer this over dumping a whole multi-step plan in a single message.
---

# Readable plans

A plan is only useful if the user can actually follow it. A long, flat list of twenty steps is
technically complete but hard to hold in your head — the reader loses the shape of the work and
can't tell which parts matter. The goal here is a plan that reads easily and is understood as it's
built, not one that's exhaustive but overwhelming.

This user has been explicit about two things: **group the plan into sections by feature**, and
**explain it step by step rather than all at once**. Everything below serves those two goals.

## Group by feature, not by file

Organize the plan around the *features or areas of work*, not around a raw file-by-file to-do list.
Each feature the change touches becomes its own section with a short, plain heading that names it —
e.g. "Leave request approval", "New LeaveType entity", "API endpoint", not "Edit Program.cs".

Under each section:

1. **One plain sentence of what this part does and why** — so the user understands the point before
   the details. No jargon where a normal word works.
2. **A few numbered steps** — the concrete actions for that feature. Keep each step to one clear
   action the user can picture. If a section grows past ~5 steps, it's probably two features; split it.

Keep the whole thing lean. Short sentences, no filler, no restating the obvious. A section the user
can read in ten seconds beats a paragraph they have to decode.

## Walk through it one section at a time

The most important instruction: **don't dump the entire plan in a single message.** The user has
said plainly that seeing everything at once is hard to follow. Instead, reveal the plan
progressively so each part can be understood before the next arrives.

The good pattern:

1. Start with a one-line map of the sections — just the feature names, so the user sees the shape:
   *"This has three parts: the LeaveType entity, the service, and the API endpoint. Let me walk you
   through each."*
2. Present the **first section** — its plain sentence and its steps — and then pause. Invite a
   reaction: *"Does that part make sense before I go on?"*
3. Once the user is comfortable, present the next section. Continue until the plan is complete.

This turns the plan into a short conversation the user follows along with, rather than a document
they have to study. It also surfaces disagreement early — if section one is wrong, you find out
before writing sections two and three.

Use judgment on granularity: a two-section plan can often be shown in one message with clear
headings; a five-section plan almost always wants pacing. When unsure, err toward fewer sections
per message — this user prefers smaller bites.

## Plain language over precision-speak

Explain each step the way you'd explain it to a colleague who knows the product but isn't staring at
the code. Name the feature in product terms first, then the technical detail if it's needed. Prefer
"when HR approves a request, we mark it approved and email the employee" over a bare list of method
signatures. The technical specifics still belong in the plan — just after the human-readable point,
not instead of it.

## In plan mode

The same shape applies when preparing a plan for approval: build it in feature-grouped sections with
plain-language intros. If the flow lets you check in section by section before finalizing, do that —
it matches the user's preference far better than presenting one finished block.

## Quick example

Instead of:

> 1. Add LeaveType entity 2. Add DbSet 3. Configure OnModelCreating 4. Create ILeaveTypeService
> 5. Create LeaveTypeService 6. Add DTOs 7. Add validator 8. Add mapping config 9. Add controller
> 10. Register DI 11. Build

Do this — first the map, then one section, then pause:

> This has three parts: **the data model**, **the service logic**, and **the API endpoint**. Let me
> take them one at a time.
>
> **1. The data model** — this is where a "leave type" (like *Annual* or *Sick*) gets stored in the
> database.
> 1. Add the `LeaveType` entity in Domain.
> 2. Register it in the database context and configure it.
>
> A migration will be needed for this, which you'll generate on your side.
>
> Does this part look right before I go on to the service logic?
