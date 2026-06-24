# AI Agent Instructions & Task Backlog (SF5-TM)

This document acts as the core context guide for the AI development agents (Claude Code / Antigravity CLI) working on this repository. Follow the architecture instructions, guidelines, and sequential backlog precisely.

---

## Agent Guidelines & System Context

1.  **Platform Standard:** This is a .NET 10 MVC application. Rely on top-level statements in `Program.cs`, standard C# 14 features, and EF Core 10 practices.
2.  **No SPA Overhead:** Do not introduce React, Angular, or Vue. Rely on server-rendered Razor Views (`.cshtml`) paired with **Bootstrap 5.3** (utilizing dark themes for a classic fighting game aesthetic) and **jQuery** for DOM updates and asynchronous APIs.
3.  **Mobile Compatibility:** Ensure all views are wrapped in container classes with responsive utility padding (`px-2 py-3`). Form elements must use `.form-control` and tables must use `.table-responsive` to guarantee usability on small touch screens.
4.  **Strict Security:** Score updates, player generation, and stage progression routes must be guarded by JWT cookie authentication. Read-only dashboards and bracket charts are public.

---

## Key Files to Leverage / Generate
*   `CLAUDE.md` - (For Claude Code) Add project overview, build commands (`dotnet build`, `dotnet run`), and routing specs.
*   `.agent/GEMINI.md` - (For Antigravity TUI) Add specialized agent skills and tools context.

---

## Implementation Backlog (Step-by-Step Task Tracker)

### Step 1: Base Project Setup & PostgreSQL Migrations
*   [ ] Run `dotnet new mvc -n Sf5TournamentManager` to scaffold the project.
*   [ ] Add NuGet packages:
    *   `Npgsql.EntityFrameworkCore.PostgreSQL`
    *   `Microsoft.EntityFrameworkCore.Design`
    *   `System.IdentityModel.Tokens.Jwt`
    *   `Microsoft.AspNetCore.Authentication.JwtBearer`
*   [ ] Set up DbContext and Entity Model classes based on `projectplan.md` schema definitions.
*   [ ] Configure local PostgreSQL Connection String in `appsettings.json`.
*   [ ] Generate and run the initial migration (`dotnet ef migrations add InitialCreate`).

### Step 2: JWT Security Implementation
*   [ ] Create `Admin` user generation tool or seed script inside `Program.cs` to ensure at least one default login exists.
*   [ ] Create `AuthController` handling `Login` requests. On success, generate a JWT token and attach it as an HttpOnly, Secure cookie named `SF5_AUTH_TOKEN`.
*   [ ] Configure security pipeline in `Program.cs` to extract and validate this cookie on protected routes.
*   [ ] Design the Bootstrap-based mobile login screen.

### Step 3: Player Registry & Tournament Creation
*   [ ] Create `PlayersController` (CRUD actions).
*   [ ] Implement simple Bootstrap-styled interface to register new players (Fields: Name, Main Character select menu).
*   [ ] Create `TournamentsController` with "Create Tournament" page where admins can name the event and check-box the active players.

### Step 4: Circle Method Round Robin Generator
*   [ ] Implement static helper method `RoundRobinScheduler.GenerateMatches(List<Guid> playerIds, Guid tournamentId)`:
    *   *Algorithm reminder:* If player count is odd, add a dummy/bye player ID. Rotate elements keeping the first element fixed.
*   [ ] Write action in `TournamentsController` that triggers when "Start Tournament" is clicked:
    *   Updates status to `GroupStage`.
    *   Generates the complete set of match objects in DB.

### Step 5: Admin Panel & Interactive Score Entry
*   [ ] Build Group Stage Dashboard showing standard standings table columns: Rank, Player, Points, Match Record (W-L), Games Differential.
*   [ ] Write API endpoint `POST /Tournaments/UpdateScore` taking match results and calculating outcomes.
*   [ ] Integrate a jQuery AJAX popup modal in the views. Tapping a match on a phone brings up the modal to log scores instantly without full page reloads.

### Step 6: Knockout Progression Generator (Bracket Engine)
*   [ ] Write business logic to compute top 4 seeds from Group Stage based on Points -> Head-to-Head -> Game Differential.
*   [ ] Provide controller endpoint to transition the tournament status from `GroupStage` to `KnockoutStage`.
*   [ ] Create 2 Semifinal Match entities immediately when transitioning:
    *   Semifinal 1: Seed #1 vs Seed #4.
    *   Semifinal 2: Seed #2 vs Seed #3.
*   [ ] When both Semifinals are marked complete, trigger creation of:
    *   Third-place Match (Losers).
    *   Final Match (Winners).

### Step 7: Mobile-Optimized Bracket View (Finals Map)
*   [ ] Design a clean, responsive single-elimination bracket chart using pure CSS Flexbox. Ensure it remains legible on narrow mobile displays (use scrollable overflow divs or vertical progression layouts).
*   [ ] Use jQuery to fetch bracket states dynamically.

### Step 8: Deployment to Render
*   [ ] Write a standard `.dockerignore` and multi-stage `Dockerfile` targeted for .NET 10.
*   [ ] Configure Render PostgreSQL credentials. Set environment variables safely.