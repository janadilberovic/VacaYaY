---
name: query-database
description: Query VacaYAY's MySQL database safely through the vacayay-mysql MCP server. Use whenever running SQL against the live database — it accounts for the real table names, soft-delete filters, enum-as-string columns, and forbids exposing password columns. Apply on every "query the DB", "check the data", "look at the table", or schema-inspection request.
---

# Querying the VacaYAY database

The `vacayay-mysql` MCP server runs raw SQL against the live MySQL database. Raw SQL bypasses
EF Core's model behavior, so results can mislead unless you account for the rules below. The
server is read-only (INSERT/UPDATE/DELETE are disabled) — never attempt to write.

## Tables (real names, all plural)

- `Users` — employees and HR. Columns include `Email`, `Role`, `PasswordHash`, `TempPassword`,
  `IsDeleted`.
- `LeaveTypes` — `Name`, `Color`, `IsDeleted`.
- `LeaveRequests` — `Status`, `EmployeeId` (FK → `Users`), `LeaveTypeId` (FK → `LeaveTypes`).

## Never expose password columns

**Do not `SELECT PasswordHash` or `TempPassword` from `Users`**, and never use `SELECT *` on
`Users` (it pulls them in). These are secrets; surfacing them in chat defeats the DTO discipline
the rest of the app enforces. Always list the columns you actually need. If a request seems to
require the password columns, refuse and explain instead of running it.

## Soft-delete is not automatic in raw SQL

EF hides soft-deleted rows via global query filters; raw SQL does not. Match the app's behavior:

- `Users` and `LeaveTypes` — add `WHERE IsDeleted = 0` unless deleted rows are explicitly wanted.
- `LeaveRequests` — **has no `IsDeleted` column of its own.** A request counts as deleted when its
  owning employee is deleted. To match the app, join and filter on the employee:
  `... FROM LeaveRequests lr JOIN Users u ON lr.EmployeeId = u.Id WHERE u.IsDeleted = 0`.

## Enums are stored as strings

`Users.Role`, `LeaveTypes.Name`, `LeaveTypes.Color`, and `LeaveRequests.Status` are persisted as
their string names (max 20 chars), not integers. Filter with the string value —
`WHERE Status = 'Approved'`, not `WHERE Status = 1`. Note `LeaveTypes.Name` is itself an enum
value, not free text.

## When results contradict the code

If the live schema or data disagrees with the entities/migrations, say so plainly — surfacing a
drift between the model and the real database is a useful finding, not something to paper over.
