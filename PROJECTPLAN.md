# Street Fighter 5 Tournament Manager (SF5-TM) - Project Plan

This document outlines the technical specification, system architecture, database design, and release roadmap for the Street Fighter 5 Tournament Management Application. The goal is to build a lightweight, mobile-responsive, and easily upgradable application for managing local tournaments.

---

## 1. Technical Stack

*   **Backend Framework:** ASP.NET Core 10 (MVC Pattern)
*   **Database:** PostgreSQL 18
*   **Data Access:** Entity Framework Core (EF Core) 10 (Code-First)
*   **Authentication:** JSON Web Token (JWT) secured via HttpOnly Cookie (for simple integration with standard ASP.NET Core MVC controllers and jQuery frontend requests)
*   **Frontend UI:** Bootstrap 5.3 (Mobile-first responsive design, dark mode theme fitting for fighting games)
*   **Frontend Scripting:** jQuery 3.7+ (for asynchronous score updates, live point tables, and dynamic brackets)
*   **Development Tools:** Claude Code (TUI agent) & Antigravity CLI (Agent-first IDE execution)
*   **Deployment Host:** Render (Web Service + Managed PostgreSQL)

---

## 2. System Architecture & Database Design

The application will use a structured MVC pattern on the backend, serving razor views equipped with responsive Bootstrap classes. Dynamic elements (like updating match scores without full-page reloads) will be handled via jQuery AJAX calls.

### Entity Relationship Model (ERD draft)

#### 1. `User` (Admin Auth)
*   `Id` (Guid, PK)
*   `Username` (string, Unique)
*   `PasswordHash` (string)
*   `CreatedAt` (DateTime)

#### 2. `Player` (Global Directory)
*   `Id` (Guid, PK)
*   `Name` (string, Unique)
*   `MainCharacter` (string) - *e.g., Ryu, Chun-Li, Akuma*
*   `CreatedAt` (DateTime)

#### 3. `Tournament`
*   `Id` (Guid, PK)
*   `Name` (string)
*   `Status` (Enum: `Draft`, `GroupStage`, `KnockoutStage`, `Finished`)
*   `CreatedAt` (DateTime)

#### 4. `TournamentPlayer` (Join Table with Statistics)
*   `TournamentId` (Guid, FK)
*   `PlayerId` (Guid, FK)
*   `GroupPoints` (int) - *1 point per Match win*
*   `GamesWon` (int) - *Sum of individual round scores, e.g., if set score is 2-1, GamesWon += 2*
*   `GamesLost` (int) - *GamesLost += 1*

#### 5. `Match`
*   `Id` (Guid, PK)
*   `TournamentId` (Guid, FK)
*   `Player1Id` (Guid, FK, Nullable for bracket seeds)
*   `Player2Id` (Guid, FK, Nullable for bracket seeds)
*   `Player1Score` (int, Nullable)
*   `Player2Score` (int, Nullable)
*   `Stage` (Enum: `Group`, `Semifinal`, `ThirdPlace`, `Final`)
*   `RoundNumber` (int) - *For Group Stage scheduling*
*   `IsCompleted` (bool)
*   `WinnerId` (Guid, FK, Nullable)
*   `CreatedAt` (DateTime)

---

## 3. Tournament Lifecycle & Core Logic

### Phase A: Group Stage (Round Robin)
1.  **Creation:** Admin creates a tournament and registers players (minimum 4 players).
2.  **Scheduling:** Upon starting the tournament, the system runs a Round Robin scheduling algorithm (such as the standard Berger Tables / Circle Method) to generate all group stage matches.
3.  **Scoring:** Every match consists of a set of games (usually Best of 3). The admin inputs the final game score (e.g., `2 - 1` or `2 - 0`).
    *   Match Winner gets **1 point** on the scoreboard.
    *   Games Won and Games Lost are tracked for tiebreakers.
4.  **Tiebreaker Priority:**
    1.  Highest total Group Points.
    2.  Head-to-head match result (if 2 players are tied).
    3.  Highest game differential (`GamesWon - GamesLost`).

### Phase B: Knockout Stage (Finals Map)
1.  **Advancement:** Admin concludes the Group Stage. The system automatically ranks the players and takes the **Top 4**.
2.  **Bracket Generation:** The system instantiates 4 bracket matches:
    *   **Semifinal 1 (SF1):** Seed #1 vs Seed #4
    *   **Semifinal 2 (SF2):** Seed #2 vs Seed #3
3.  **Consolation & Finals Placement:** Once both Semifinals are resolved:
    *   **Third Place Match:** Loser SF1 vs Loser SF2
    *   **Grand Final:** Winner SF1 vs Winner SF2
4.  **Completion:** Once the Grand Final is logged, the tournament is marked `Finished`.

---

## 4. Phase-by-Phase Roadmap

### Phase 1: Scaffolding & DB Context
*   Initialize ASP.NET Core 10 MVC template.
*   Setup Entity Framework Core with PostgreSQL.
*   Configure DB connection string structures compatible with local secrets and production environment variables.
*   Build DB Migrations for `Users`, `Players`, `Tournaments`, `TournamentPlayers`, and `Matches`.

### Phase 2: Authentication (JWT in Cookies)
*   Create registration and login actions.
*   Implement custom JWT token generation.
*   Use standard cookie middleware configured to extract/validate JWT tokens, securing admin controllers without requiring complex SPA auth states on the frontend.

### Phase 3: Player & Tournament Management UI
*   Create mobile-responsive forms with Bootstrap to Add/Edit Players and Create Tournaments.
*   Configure safe controller logic ensuring only logged-in Admins can modify data.

### Phase 4: Round Robin Execution Engine
*   Build scheduling helper class to calculate match pairings.
*   Develop the Tournament Detail view containing:
    *   Active Point Table (Group standings).
    *   Match listing grouped by rounds.
*   Implement AJAX-driven modal for Admins to quickly log scores from mobile screens.

### Phase 5: Knockout Bracket & Statistics View
*   Build bracket generation endpoint when advancing to Semifinals.
*   Create a clean, visual responsive bracket display (Finals map) using Flexbox CSS, optimized for vertical/horizontal mobile scrolling.
*   Build "Player Statistics" views summarizing overall win percentages and character pick frequencies.

### Phase 6: Render Deployment
*   Prepare Dockerfile or Render Native build configurations for .NET 10.
*   Provision Render Managed PostgreSQL 18 instance.
*   Configure environment variables (`ConnectionStrings__DefaultConnection` and JWT secret security keys).
*   Run database migrations during startup or build step.