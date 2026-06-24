namespace SF5_Tournament.Models;

/// <summary>Join of a player into a tournament, carrying group-stage statistics.</summary>
public class TournamentPlayer
{
    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    /// <summary>1 point per match won.</summary>
    public int GroupPoints { get; set; }

    /// <summary>Sum of game scores won across all logged sets.</summary>
    public int GamesWon { get; set; }

    /// <summary>Sum of game scores lost across all logged sets.</summary>
    public int GamesLost { get; set; }

    /// <summary>GamesWon - GamesLost. Tiebreaker of last resort.</summary>
    public int GameDifferential => GamesWon - GamesLost;
}
