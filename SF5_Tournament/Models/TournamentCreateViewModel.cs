using System.ComponentModel.DataAnnotations;

namespace SF5_Tournament.Models;

public class TournamentCreateViewModel
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Ids of players to enroll in this tournament.</summary>
    public List<Guid> SelectedPlayerIds { get; set; } = new();
}
