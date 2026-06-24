# 2026-06-24 — Initial Build & Render Deploy

**Agent:** Claude Code (model: glm-5.2)
**Scope:** Full implementation of SF5-TM from spec (`PROJECTPLAN.md` + `AGENTS.md`) through live deploy.

## Intent
Turn the default `dotnet new mvc` scaffold (SQLite + ASP.NET Identity) into the tournament system the spec describes (PostgreSQL + custom JWT-cookie auth, round-robin groups, live scoring, Top-4 knockout bracket) and ship it to Render.

## Key decisions
- **Auth:** stripped ASP.NET Identity entirely; custom `User` entity + JWT issued into HttpOnly cookie `SF5_AUTH_TOKEN`, validated via JwtBearer reading the cookie. `PasswordHasher<T>` reused from the shared framework (no Identity package needed).
- **DB:** PostgreSQL on Render (`sf5db`). External URL for local dev, internal URL for prod.
- **Admin:** seeded from config/env on startup if absent (`DbSeeder`).
- **Standings source of truth:** computed from completed group matches (`StandingsService`); `TournamentPlayer` stat columns kept as a persisted cache, refreshed on each score entry.

## Files added
- `SF5_Tournament/Models/`: `User.cs`, `Player.cs`, `Tournament.cs`, `TournamentPlayer.cs`, `Match.cs`, `Enums.cs`, `SF5Roster.cs`, `PlayerViewModel.cs`, `TournamentCreateViewModel.cs`, `UpdateScoreViewModel.cs`, `LoginViewModel.cs`, `StandingRow.cs`.
- `SF5_Tournament/Services/`: `JwtTokenService.cs`, `DbSeeder.cs`, `RoundRobinScheduler.cs`, `StandingsService.cs`, `BracketService.cs`.
- `SF5_Tournament/Controllers/`: `AuthController.cs`, `PlayersController.cs`, `TournamentsController.cs`.
- `SF5_Tournament/Views/`: `Auth/Login.cshtml`, `Players/{Index,Create,Edit}.cshtml`, `Tournaments/{Index,Create,Detail,Bracket,_StandingsRows,_BracketSlot}.cshtml`, rewrote `Home/Index.cshtml`, `Shared/_Layout.cshtml`, `Shared/_LoginPartial.cshtml`.
- `Data/Migrations/*_InitialCreate.cs` (Npgsql).
- `Dockerfile` (multi-stage net10, port 10000), `.dockerignore`.

## Files changed
- `SF5_Tournament.csproj` — swapped Sqlite/Identity packages for Npgsql/JwtBearer/Design/Tools.
- `SF5_Tournament/Program.cs` — Npgsql, JwtBearer-from-cookie pipeline, `Migrate()` + `SeedAdminAsync` on startup, **URL→key=value connection-string normalizer** (deploy fix).
- `Data/ApplicationDbContext.cs` — `DbContext` with 5 DbSets + explicit FK config.
- `appsettings*.json`, `.gitignore` (added `postgres.md`, `*.db`).

## Files deleted
- `Areas/` (Identity UI), `app.db`, old `CreateIdentitySchema` migration + snapshot, default `_LoginPartial.cshtml`.

## Deploy
- Render web service `srv-d8tqlue7r5hc73al22kg`, Singapore, Docker, free tier. URL: https://sf5-tournament.onrender.com
- Env vars set: `ConnectionStrings__DefaultConnection`, `Jwt__Secret`, `Admin__Password`, `ASPNETCORE_ENVIRONMENT`.
- **Bug fixed:** first deploy crashed (exit 139) — `System.ArgumentException: initialization string does not conform` because the Render DB URL is `postgresql://` form and `UseNpgsql` expects key=value. Fixed by `NormalizeConnectionString` in `Program.cs` (commit `e0f9b8d`). Re-deploy went **live**.
- Git remote switched HTTPS→SSH (`git@github.com:shefat2002/SF5_Tournament.git`) to enable push (no HTTPS creds on machine).

## Verify status
- `dotnet build` — clean (0 errors).
- Local smoke: `/` 200, `/Auth/Login` 200, `/Players` 401 to anon; admin INSERTed to Postgres.
- Prod smoke: same 200/200/401; logs `Application started` + `Hosting environment: Production` + `service is live`.

## Follow-ups
- Suppress harmless `Failed to determine the https port for redirect` warning (add `UseForwardedHeaders` or drop `UseHttpsRedirection`).
- Manual end-to-end tournament playthrough in browser (not yet run).
- UI restyle to light + red/yellow arcade theme (separate doc entry).
