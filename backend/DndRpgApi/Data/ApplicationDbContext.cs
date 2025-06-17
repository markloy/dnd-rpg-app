using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DndRpgApi.Models;

namespace DndRpgApi.Data
{
    // DbContext is the main class that coordinates EF functionality
    // IdentityDbContext provides user management tables automatically
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor - dependency injection provides DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties represent tables in the database
        // EF Core creates tables based on these properties
        public DbSet<Character> Characters { get; set; }
        public DbSet<Monster> Monsters { get; set; }

        // Configure the model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call base method to configure Identity tables
            base.OnModelCreating(modelBuilder);

            // Configure Character entity
            modelBuilder.Entity<Character>(entity =>
            {
                // Table name 
                entity.ToTable("Characters");

                // Primary key 
                entity.HasKey(e => e.Id);

                // Configure the relationship between User and Character
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Characters)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete characters when user is deleted

                // Index for performance on UserId lookups
                entity.HasIndex(e => e.UserId);

                // Default values
                entity.Property(e => e.Level).HasDefaultValue(1);
                entity.Property(e => e.Health).HasDefaultValue(100);
                entity.Property(e => e.MaxHealth).HasDefaultValue(100);
                entity.Property(e => e.Gold).HasDefaultValue(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Monster entity
            modelBuilder.Entity<Monster>(entity =>
            {
                entity.ToTable("Monsters");
                entity.HasKey(e => e.Id);

                // Unique constraint on ApiId (each monster from D&D API appears once)
                entity.HasIndex(e => e.ApiId).IsUnique();

                // Index for performance on challenge rating queries
                entity.HasIndex(e => e.ChallengeRating);

                // Configure decimal precision for challenge rating
                entity.Property(e => e.ChallengeRating)
                      .HasColumnType("decimal(4,2)"); // Allows 0.25, 0.5, 1.0, etc.

                // Default values
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Seed data 
            SeedData(modelBuilder);
        }

        // Override SaveChanges to automatically update UpdatedAt timestamps
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Character || e.Entity is Monster)
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Character character)
                {
                    character.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Monster monster)
                {
                    monster.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            
            // For now, just a placeholder to show the concept
        }
    }
}