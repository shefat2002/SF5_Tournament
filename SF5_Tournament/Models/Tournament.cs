using System.ComponentModel.DataAnnotations;

namespace SF5_Tournament.Models;

/// <summary>A single tournament event.</summary>
public class Tournament
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TournamentPlayer> Participants { get; set; } = new List<TournamentPlayer>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
