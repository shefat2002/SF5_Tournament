using Microsoft.EntityFrameworkCore;
using SF5_Tournament.Models;

namespace SF5_Tournament.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentPlayer> TournamentPlayers => Set<TournamentPlayer>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.HasIndex(p => p.Name).IsUnique();
        });

        // Join table with composite key + statistics.
        modelBuilder.Entity<TournamentPlayer>(e =>
        {
            e.HasKey(tp => new { tp.TournamentId, tp.PlayerId });
            e.HasOne(tp => tp.Tournament)
                .WithMany(t => t.Participants)
                .HasForeignKey(tp => tp.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(tp => tp.Player)
                .WithMany()
                .HasForeignKey(tp => tp.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Ignore(tp => tp.GameDifferential); // computed, not persisted
        });

        // A Match has up to three nullable FKs into Player (P1, P2, Winner) —
        // configure each explicitly so EF does not guess the relationship.
        modelBuilder.Entity<Match>(e =>
        {
            e.HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Player1)
                .WithMany()
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Player2)
                .WithMany()
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Winner)
                .WithMany()
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
