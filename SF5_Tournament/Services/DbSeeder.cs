using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SF5_Tournament.Data;
using SF5_Tournament.Models;

namespace SF5_Tournament.Services;

public record AdminSeed(string Username, string Password);

/// <summary>Ensures configured admin accounts exist on startup.</summary>
public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var hasher = new PasswordHasher<User>();

        // Backward-compatible single admin (Admin:Username / Admin:Password).
        await EnsureUserAsync(db, hasher, config["Admin:Username"], config["Admin:Password"]);

        // Additional admins from the Admins:[ {Username, Password} ] config list.
        var admins = config.GetSection("Admins").Get<List<AdminSeed>>() ?? new List<AdminSeed>();
        foreach (var a in admins)
        {
            await EnsureUserAsync(db, hasher, a.Username, a.Password);
        }
    }

    private static async Task EnsureUserAsync(
        ApplicationDbContext db, PasswordHasher<User> hasher, string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (await db.Users.AnyAsync(u => u.Username == username))
        {
            return;
        }

        var user = new User { Username = username };
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
