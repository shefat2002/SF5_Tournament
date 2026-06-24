using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SF5_Tournament.Data;
using SF5_Tournament.Models;

namespace SF5_Tournament.Services;

/// <summary>Ensures a default admin account exists on startup.</summary>
public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var config = services.GetRequiredService<IConfiguration>();

        var username = config["Admin:Username"] ?? "admin";
        var password = config["Admin:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            return; // nothing to seed without a configured password
        }

        if (await db.Users.AnyAsync(u => u.Username == username))
        {
            return;
        }

        var hasher = new PasswordHasher<User>();
        var user = new User { Username = username };
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
