using System.ComponentModel.DataAnnotations;

namespace SF5_Tournament.Models;

/// <summary>Admin account allowed to mutate tournament data.</summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash produced by <c>Microsoft.AspNetCore.Identity.PasswordHasher&lt;User&gt;</c>.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
