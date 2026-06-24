using SF5_Tournament.Models;

namespace SF5_Tournament.Services;

/// <summary>Computes group standings from completed group matches.</summary>
public static class StandingsService
{
    public static List<StandingRow> Compute(Tournament tournament)
    {
        var rows = tournament.Participants.ToDictionary(
            tp => tp.PlayerId,
            tp => new StandingRow
            {
                PlayerId = tp.PlayerId,
                Name = tp.Player.Name,
                Character = tp.Player.MainCharacter
            });

        foreach (var m in tournament.Matches.Where(x => x.Stage == MatchStage.Group && x.IsCompleted))
        {
            if (m.Player1Id is null || m.Player2Id is null ||
                m.Player1Score is null || m.Player2Score is null)
            {
                continue;
            }

            var p1 = m.Player1Id.Value;
            var p2 = m.Player2Id.Value;
            var s1 = m.Player1Score.Value;
            var s2 = m.Player2Score.Value;

            if (rows.TryGetValue(p1, out var r1))
            {
                r1.GamesWon += s1;
                r1.GamesLost += s2;
                if (s1 > s2) r1.Wins++; else r1.Losses++;
            }

            if (rows.TryGetValue(p2, out var r2))
            {
                r2.GamesWon += s2;
                r2.GamesLost += s1;
                if (s2 > s1) r2.Wins++; else r2.Losses++;
            }
        }

        foreach (var row in rows.Values)
        {
            row.Points = row.Wins;
            row.GameDiff = row.GamesWon - row.GamesLost;
        }

        return rows.Values
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.GameDiff)
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .Select((r, i) => { r.Rank = i + 1; return r; })
            .ToList();
    }
}
