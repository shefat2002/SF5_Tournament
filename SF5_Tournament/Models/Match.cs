using System.ComponentModel.DataAnnotations;

namespace SF5_Tournament.Models;

/// <summary>A single pairing in a tournament (group round or knockout slot).</summary>
public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    /// <summary>Null when a knockout slot has not yet been seeded.</summary>
    public Guid? Player1Id { get; set; }
    public Player? Player1 { get; set; }

    public Guid? Player2Id { get; set; }
    public Player? Player2 { get; set; }

    public int? Player1Score { get; set; }
    public int? Player2Score { get; set; }

    public MatchStage Stage { get; set; } = MatchStage.Group;

    /// <summary>Group-stage scheduling round (0 for knockout matches).</summary>
    public int RoundNumber { get; set; }

    public bool IsCompleted { get; set; }

    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
