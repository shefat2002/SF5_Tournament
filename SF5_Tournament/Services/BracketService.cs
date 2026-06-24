using Microsoft.EntityFrameworkCore;
using SF5_Tournament.Data;
using SF5_Tournament.Models;

namespace SF5_Tournament.Services;

/// <summary>
/// Top-4 knockout bracket: seeds group results, creates semifinals, and grows the
/// bracket into third-place + final as semifinals resolve.
/// </summary>
public static class BracketService
{
    /// <summary>Rank players by Points → Head-to-Head (2-way ties) → Game differential, take Top 4.</summary>
    public static async Task<List<StandingRow>> ComputeSeeds(ApplicationDbContext db, Guid tournamentId)
    {
        var tournament = await db.Tournaments
            .Include(t => t.Participants).ThenInclude(tp => tp.Player)
            .Include(t => t.Matches)
            .FirstAsync(t => t.Id == tournamentId);

        var ranked = StandingsService.Compute(tournament);
        ApplyHeadToHead(ranked, tournament);
        return ranked.Take(4).ToList();
    }

    /// <summary>Creates semifinal matches for the Top 4 seeds and flips the tournament to knockout.</summary>
    public static async Task AdvanceToKnockout(ApplicationDbContext db, Guid tournamentId)
    {
        var tournament = await db.Tournaments
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament is null || tournament.Status != TournamentStatus.GroupStage)
        {
            return;
        }

        if (tournament.Matches.Any(m => m.Stage != MatchStage.Group))
        {
            return; // already bracketed
        }

        var seeds = await ComputeSeeds(db, tournamentId);
        if (seeds.Count < 4)
        {
            return;
        }

        // SF1: seed 1 vs seed 4, SF2: seed 2 vs seed 3.
        db.Matches.Add(new Match
        {
            TournamentId = tournamentId,
            Player1Id = seeds[0].PlayerId,
            Player2Id = seeds[3].PlayerId,
            Stage = MatchStage.Semifinal,
            RoundNumber = 1
        });
        db.Matches.Add(new Match
        {
            TournamentId = tournamentId,
            Player1Id = seeds[1].PlayerId,
            Player2Id = seeds[2].PlayerId,
            Stage = MatchStage.Semifinal,
            RoundNumber = 2
        });

        tournament.Status = TournamentStatus.KnockoutStage;
        await db.SaveChangesAsync();
    }

    /// <summary>Called after a knockout match is scored; grows the bracket or finishes the tournament.</summary>
    public static async Task OnKnockoutCompleted(ApplicationDbContext db, Match match)
    {
        if (match.Stage == MatchStage.Semifinal)
        {
            var semifinals = await db.Matches
                .Where(m => m.TournamentId == match.TournamentId && m.Stage == MatchStage.Semifinal)
                .OrderBy(m => m.RoundNumber)
                .ToListAsync();

            if (semifinals.Count != 2 || semifinals.Any(m => !m.IsCompleted))
            {
                return;
            }

            var alreadyHasFinals = await db.Matches.AnyAsync(m =>
                m.TournamentId == match.TournamentId &&
                (m.Stage == MatchStage.Final || m.Stage == MatchStage.ThirdPlace));

            if (alreadyHasFinals)
            {
                return;
            }

            var (w1, l1) = WinnerLoser(semifinals[0]);
            var (w2, l2) = WinnerLoser(semifinals[1]);
            if (w1 is null || w2 is null || l1 is null || l2 is null)
            {
                return;
            }

            db.Matches.Add(new Match
            {
                TournamentId = match.TournamentId,
                Player1Id = w1,
                Player2Id = w2,
                Stage = MatchStage.Final
            });
            db.Matches.Add(new Match
            {
                TournamentId = match.TournamentId,
                Player1Id = l1,
                Player2Id = l2,
                Stage = MatchStage.ThirdPlace
            });

            await db.SaveChangesAsync();
        }
        else if (match.Stage == MatchStage.Final)
        {
            var tournament = await db.Tournaments.FindAsync(match.TournamentId);
            if (tournament is not null && tournament.Status != TournamentStatus.Finished)
            {
                tournament.Status = TournamentStatus.Finished;
                await db.SaveChangesAsync();
            }
        }
    }

    private static void ApplyHeadToHead(List<StandingRow> ranked, Tournament tournament)
    {
        var headToHead = new Dictionary<string, Guid>();
        foreach (var m in tournament.Matches.Where(x => x.Stage == MatchStage.Group && x.IsCompleted))
        {
            if (m.Player1Id is null || m.Player2Id is null || m.WinnerId is null)
            {
                continue;
            }
            headToHead[PairKey(m.Player1Id.Value, m.Player2Id.Value)] = m.WinnerId.Value;
        }

        for (var i = 0; i + 1 < ranked.Count; i++)
        {
            if (ranked[i].Points != ranked[i + 1].Points)
            {
                continue;
            }
            var winner = headToHead.GetValueOrDefault(PairKey(ranked[i].PlayerId, ranked[i + 1].PlayerId));
            if (winner == ranked[i + 1].PlayerId)
            {
                (ranked[i], ranked[i + 1]) = (ranked[i + 1], ranked[i]);
            }
        }

        for (var i = 0; i < ranked.Count; i++)
        {
            ranked[i].Rank = i + 1;
        }
    }

    private static string PairKey(Guid a, Guid b)
    {
        return a.CompareTo(b) < 0 ? $"{a}|{b}" : $"{b}|{a}";
    }

    private static (Guid? Winner, Guid? Loser) WinnerLoser(Match m)
    {
        if (!m.IsCompleted || m.WinnerId is null || m.Player1Id is null || m.Player2Id is null)
        {
            return (null, null);
        }
        var winner = m.WinnerId.Value;
        var loser = winner == m.Player1Id.Value ? m.Player2Id.Value : m.Player1Id.Value;
        return (winner, loser);
    }
}
