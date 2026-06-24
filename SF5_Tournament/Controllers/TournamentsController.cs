using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SF5_Tournament.Data;
using SF5_Tournament.Models;
using SF5_Tournament.Services;

namespace SF5_Tournament.Controllers;

[Authorize]
public class TournamentsController(ApplicationDbContext db) : Controller
{
    private const int MinPlayers = 4;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var tournaments = await db.Tournaments
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        return View(tournaments);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Players = await db.Players.OrderBy(p => p.Name).ToListAsync();
        ViewBag.Characters = SF5Roster.Characters;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TournamentCreateViewModel model)
    {
        ViewBag.Players = await db.Players.OrderBy(p => p.Name).ToListAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.SelectedPlayerIds.Count < MinPlayers)
        {
            ModelState.AddModelError(nameof(model.SelectedPlayerIds),
                $"Select at least {MinPlayers} players.");
            return View(model);
        }

        var tournament = new Tournament { Name = model.Name.Trim() };

        foreach (var playerId in model.SelectedPlayerIds.Distinct())
        {
            tournament.Participants.Add(new TournamentPlayer { PlayerId = playerId });
        }

        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Detail), new { id = tournament.Id });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Detail(Guid id)
    {
        var tournament = await db.Tournaments
            .Include(t => t.Participants).ThenInclude(tp => tp.Player)
            .Include(t => t.Matches).ThenInclude(m => m.Player1)
            .Include(t => t.Matches).ThenInclude(m => m.Player2)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament is null)
        {
            return NotFound();
        }

        ViewBag.MinPlayers = MinPlayers;
        ViewBag.Standings = StandingsService.Compute(tournament);
        return View(tournament);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid id)
    {
        var tournament = await db.Tournaments
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament is null)
        {
            return NotFound();
        }

        if (tournament.Status != TournamentStatus.Draft)
        {
            return BadRequest("Tournament already started.");
        }

        if (tournament.Participants.Count < MinPlayers)
        {
            TempData["Error"] = $"Need at least {MinPlayers} players to start.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var playerIds = tournament.Participants.Select(tp => tp.PlayerId).ToList();
        var matches = RoundRobinScheduler.GenerateMatches(playerIds, tournament.Id);
        db.Matches.AddRange(matches);

        tournament.Status = TournamentStatus.GroupStage;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Advance(Guid id)
    {
        await BracketService.AdvanceToKnockout(db, id);
        return RedirectToAction(nameof(Bracket), new { id });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Bracket(Guid id)
    {
        var tournament = await db.Tournaments
            .Include(t => t.Matches).ThenInclude(m => m.Player1)
            .Include(t => t.Matches).ThenInclude(m => m.Player2)
            .Include(t => t.Participants).ThenInclude(tp => tp.Player)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament is null)
        {
            return NotFound();
        }

        ViewBag.Seeds = tournament.Status == TournamentStatus.KnockoutStage || tournament.Status == TournamentStatus.Finished
            ? await BracketService.ComputeSeeds(db, id)
            : new List<StandingRow>();

        return View(tournament);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tournament = await db.Tournaments.FindAsync(id);
        if (tournament is null)
        {
            return NotFound();
        }

        db.Tournaments.Remove(tournament);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Logs a match result via AJAX. Group matches refresh standings; knockout matches grow the bracket.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateScore([FromBody] UpdateScoreViewModel model)
    {
        if (model is null || model.MatchId == Guid.Empty)
        {
            return Json(new { success = false, error = "Invalid request." });
        }

        var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == model.MatchId);
        if (match is null)
        {
            return NotFound();
        }

        if (match.Player1Id is null || match.Player2Id is null)
        {
            return Json(new { success = false, error = "Match not seeded." });
        }

        if (model.Player1Score < 0 || model.Player2Score < 0)
        {
            return Json(new { success = false, error = "Scores cannot be negative." });
        }

        if (model.Player1Score == model.Player2Score)
        {
            return Json(new { success = false, error = "A match cannot end in a tie." });
        }

        match.Player1Score = model.Player1Score;
        match.Player2Score = model.Player2Score;
        match.WinnerId = model.Player1Score > model.Player2Score ? match.Player1Id : match.Player2Id;
        match.IsCompleted = true;

        if (match.Stage == MatchStage.Group)
        {
            await RecomputeGroupCachesAsync(match.TournamentId);
            await db.SaveChangesAsync();

            var tournament = await db.Tournaments
                .Include(t => t.Participants).ThenInclude(tp => tp.Player)
                .Include(t => t.Matches)
                .FirstAsync(t => t.Id == match.TournamentId);

            var standings = StandingsService.Compute(tournament);
            var allGroupDone = tournament.Matches.Where(m => m.Stage == MatchStage.Group).All(m => m.IsCompleted);

            return Json(new
            {
                success = true,
                knockout = false,
                winnerId = match.WinnerId,
                canAdvance = allGroupDone && tournament.Status == TournamentStatus.GroupStage,
                standings = standings.Select(s => new { s.Rank, s.PlayerId, s.Name, s.Points, s.Wins, s.Losses, s.GameDiff })
            });
        }

        // Knockout: persist, then grow the bracket (or finish) and tell the client to refresh.
        await db.SaveChangesAsync();
        await BracketService.OnKnockoutCompleted(db, match);

        var status = await db.Tournaments.Where(t => t.Id == match.TournamentId).Select(t => t.Status).FirstAsync();
        return Json(new { success = true, knockout = true, finished = status == TournamentStatus.Finished });
    }

    private async Task RecomputeGroupCachesAsync(Guid tournamentId)
    {
        var participants = await db.TournamentPlayers
            .Where(tp => tp.TournamentId == tournamentId).ToListAsync();
        var groupMatches = await db.Matches
            .Where(m => m.TournamentId == tournamentId && m.Stage == MatchStage.Group).ToListAsync();

        foreach (var tp in participants)
        {
            tp.GroupPoints = 0;
            tp.GamesWon = 0;
            tp.GamesLost = 0;
        }

        var byPlayer = participants.ToDictionary(tp => tp.PlayerId);
        foreach (var m in groupMatches.Where(x => x.IsCompleted
                                                  && x.Player1Id.HasValue && x.Player2Id.HasValue
                                                  && x.Player1Score.HasValue && x.Player2Score.HasValue))
        {
            var s1 = m.Player1Score!.Value;
            var s2 = m.Player2Score!.Value;
            if (byPlayer.TryGetValue(m.Player1Id!.Value, out var a))
            {
                a.GamesWon += s1;
                a.GamesLost += s2;
                if (s1 > s2) a.GroupPoints++;
            }
            if (byPlayer.TryGetValue(m.Player2Id!.Value, out var b))
            {
                b.GamesWon += s2;
                b.GamesLost += s1;
                if (s2 > s1) b.GroupPoints++;
            }
        }
    }
}
