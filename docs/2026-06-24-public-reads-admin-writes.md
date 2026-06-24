# 2026-06-24 — Public Reads, Admin-Only Writes, +sgazifahim Admin

**Agent:** Claude Code
**Scope:** Open read access to everyone; restrict writes to logged-in admins; add a second admin account.

## Intent
Spectators should browse the active tournament list, dashboards, score tables, and roster without logging in. Only admins create tournaments/players and log scores. Add admin `sgazifahim`.

## Changes
- **Controllers** (`PlayersController`, `TournamentsController`): kept class-level `[Authorize]`; marked reads `[AllowAnonymous]` — `Players.Index`, `Tournaments.Index`, `Tournaments.Detail`, `Tournaments.Bracket`. All create/edit/delete/start/advance/`UpdateScore` remain `[Authorize]`.
- **Views**: gated write UI behind `User.Identity?.IsAuthenticated`:
  - `Players/Index` — Add/Edit/Delete only for admins.
  - `Tournaments/Index` — New Tournament / Delete only for admins.
  - `Tournaments/Detail` — Start, Advance, match "enter" buttons, and the score modal/script only for admins; standings + matches stay public.
  - `Tournaments/Bracket` + `_BracketSlot` — score "enter" buttons only for admins.
- **`Services/DbSeeder.cs`**: now seeds a configurable `Admins:[ {Username, Password} ]` list in addition to the legacy single `Admin:Username/Password`. Added `AdminSeed` record + `EnsureUserAsync` helper (idempotent).
- **Render env**: added `Admins__0__Username=sgazifahim`, `Admins__0__Password=sgazifahim123`.

## Verify (prod, commit `5fc16f2`, deploy `dep-d8trcut8nd3s73erkb6g` live)
- Anon: `/Players` 200, `/Tournaments` 200, `/Auth/Login` 200.
- Anon writes: `/Players/Create` 401, `/Tournaments/Create` 401.
- `sgazifahim` login POST → 302, `SF5_AUTH_TOKEN` cookie set; authed `/Players/Create` → 200.

## Notes
- `sgazifahim123` is weak — flagged to user; their choice.
- Commit: `5fc16f2`.

## Follow-ups
- Could hide `Draft` tournaments from the public list (only show active). Currently all statuses visible; writes stay gated.
