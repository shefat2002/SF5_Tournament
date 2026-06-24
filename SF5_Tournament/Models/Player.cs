using System.ComponentModel.DataAnnotations;

namespace SF5_Tournament.Models;

/// <summary>Global player directory entry, reused across tournaments.</summary>
public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Main character pick, e.g. "Ryu". See <see cref="SF5Roster"/>.</summary>
    [MaxLength(40)]
    public string MainCharacter { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
