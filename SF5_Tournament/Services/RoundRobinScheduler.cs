using SF5_Tournament.Models;

namespace SF5_Tournament.Services;

/// <summary>
/// Circle-method round-robin generator. Fixes one player and rotates the rest,
/// producing a balanced schedule where every player meets every other exactly once.
/// </summary>
public static class RoundRobinScheduler
{
    /// <summary>Sentinel id marking a bye slot (odd player count).</summary>
    public static readonly Guid ByeId = Guid.Empty;

    public static List<Match> GenerateMatches(List<Guid> playerIds, Guid tournamentId)
    {
        var ids = playerIds.ToList();
        if (ids.Count < 2)
        {
            return new List<Match>();
        }

        // Pad with a bye so rotation works for odd counts.
        if (ids.Count % 2 != 0)
        {
            ids.Add(ByeId);
        }

        var n = ids.Count;
        var rounds = n - 1;
        var half = n / 2;
        var rotation = ids.ToList();
        var matches = new List<Match>();

        for (var round = 1; round <= rounds; round++)
        {
            for (var i = 0; i < half; i++)
            {
                var a = rotation[i];
                var b = rotation[n - 1 - i];

                if (a == ByeId || b == ByeId)
                {
                    continue; // bye round — no match recorded
                }

                // Alternate sides between rounds for home/away fairness.
                var (p1, p2) = round % 2 == 1 ? (a, b) : (b, a);
                matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    Player1Id = p1,
                    Player2Id = p2,
                    Stage = MatchStage.Group,
                    RoundNumber = round
                });
            }

            Rotate(rotation);
        }

        return matches;
    }

    /// <summary>Keep index 0 fixed, move the last element into position 1.</summary>
    private static void Rotate(List<Guid> rotation)
    {
        var last = rotation[^1];
        rotation.RemoveAt(rotation.Count - 1);
        rotation.Insert(1, last);
    }
}
