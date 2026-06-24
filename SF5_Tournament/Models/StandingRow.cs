namespace SF5_Tournament.Models;

/// <summary>Computed group-stage standing row (not persisted).</summary>
public class StandingRow
{
    public int Rank { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GameDiff { get; set; }
}
