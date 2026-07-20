---
name: use-api-endpoints
description: On the VacaYAY frontend (web/), consume the backend API for any data or rule it already owns instead of reimplementing that logic client-side. Use whenever writing or editing frontend code that computes, validates, or derives something the API already returns or enforces — working-day counts, leave-balance checks, overlap/eligibility rules, holiday lists, status transitions, formatting the backend already does. Apply on every frontend "add", "implement", "compute", "validate", or "check" task, not only when the user mentions the API.
---

# Use the API, don't re-implement it

The backend is the single source of truth. It validates input, enforces the business rules, and
returns computed values. The frontend's job is to **call the endpoint and render the result** — not
to re-derive that same value or re-check that same rule with a parallel client-side implementation.

A second implementation on the frontend is a liability, not a convenience: it can silently disagree
with the server, it has to be kept in sync by hand when the rule changes, and there's usually no test
pinning the two together. When the API already owns a piece of data or logic, reach for the endpoint.

## The rule

Before writing a frontend function that computes, validates, or derives something, ask:
**does the backend already return or enforce this?** If yes — call it.

- **Data the API returns → read it from the response, don't recompute it.**
  `LeaveRequestDto.WorkingDays` is computed server-side in
  `src/VacaYAY.Business/Services/LeaveRequest/LeaveRequestService.cs` (`CountWorkingDays`). On the
  frontend, take `dto.workingDays` — don't write your own loop over the date range.
- **Rules the API enforces → let the request fail and surface the error, don't pre-validate to gate it.**
  Overlap rejection, leave-type existence, and insufficient-balance checks all live in
  `CreateAsync`/`ApproveAsync`. Don't re-encode those rules on the frontend to decide whether to send
  the request — send it, and render the `ApiError.firstMessage` if it comes back 400/409.
- **Reference lists the API owns → fetch them, don't hardcode or regenerate them.**
  Public holidays come from `GET /leave-requests/holidays?year=` (`leaveRequests.holidays` in
  [web/lib/endpoints.ts](../../../web/lib/endpoints.ts)). Never hardcode a holiday table or
  compute holidays client-side — the good example is [web/lib/holidays.ts](../../../web/lib/holidays.ts),
  which fetches and caches per year.

## How to consume an endpoint here

The frontend already has a clean API layer — use it, don't hand-roll `fetch`:

1. Add the call to the right group in [web/lib/endpoints.ts](../../../web/lib/endpoints.ts)
   (`auth`, `leaveRequests`, `employees`, `leaveTypes`), typed via `api.get`/`api.post`/etc. from
   [web/lib/api.ts](../../../web/lib/api.ts). That layer already attaches the bearer token and maps
   RFC7807 problems to `ApiError`.
2. If the endpoint doesn't exist yet, that's a **backend** change — add the controller action +
   Business service method first (see the `add-feature` skill), then expose it in `endpoints.ts`.
   Don't route around a missing endpoint by computing the answer on the client.
3. Render the returned value or the `ApiError` — no shadow copy of the server's logic.

## The one legitimate exception: instant UX preview

You may recompute a value client-side **only** for immediate in-form feedback before the round-trip
(e.g. "this request will use N working days" as the user picks dates), and only when:

- the server value stays authoritative — the preview is never persisted or trusted, and
- the inputs to the preview come from the API, not hardcoded (e.g. `estimateWorkingDays` in
  [web/lib/dates.ts](../../../web/lib/dates.ts) counts against holidays **fetched** from the API, so
  it can't drift on *which* days are holidays — only the arithmetic is local), and
- you leave a comment saying it mirrors a specific server-side method, so the next reader knows both
  must change together.

If you're adding a preview like this, prefer displaying the server's number once it returns and
treating the local estimate as provisional. When in doubt, don't duplicate — call the endpoint.

## The habit

Before finishing a frontend change, reread the diff and ask of each computation or check: *is the
backend already doing this?* If it is, delete the client copy and read the API's answer instead. A
frontend that trusts the server is smaller, can't disagree with it, and doesn't rot when the rules move.
