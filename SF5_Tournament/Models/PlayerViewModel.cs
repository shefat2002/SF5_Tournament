using System.ComponentModel.DataAnnotations;

namespace SF5_Tournament.Models;

public class PlayerViewModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string MainCharacter { get; set; } = "Ryu";
}
