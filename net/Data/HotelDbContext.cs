using Microsoft.EntityFrameworkCore;
using HotelManagement.Models;
using MongoDB.EntityFrameworkCore.Extensions;

namespace HotelManagement.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map to MongoDB Collections
        modelBuilder.Entity<User>().ToCollection("users");
        modelBuilder.Entity<Room>().ToCollection("rooms");
        modelBuilder.Entity<Reservation>().ToCollection("reservations");

        // Helper to map guid to string for better readability in mongo? 
        // Or default behavior is often acceptable (stored as Binary or String depending on GUID representation).
        // Let's stick to defaults for now, but ensure no SQL relation constraints.

        // Ignore navigation properties that might cause cyclic issues if not handled carefully in document db
        // But EF Core for MongoDB can handle some relationships. 
        // For simplicity and "NoSQL" style, we might rely less on strict foreign keys, 
        // but keeping the logical model is fine.
        
        // Remove CheckConstraints which are SQL specific
        // Remove HasForeignKey constraints which are SQL specific (EF Core Mongo might ignore or handle them purely in memory, 
        // but explicit DeleteBehavior is definitely relational).
        
        // We will keep the entity configuration minimal for MongoDB.
        
        // Configure Reservation entity
        modelBuilder.Entity<Reservation>(entity =>
        {
             // In NoSQL usually we don't enforce strict FK constraints at DB level
             // We can keep the navigation properties for EF Core to use, 
             // but 'OnDelete' behaviors are not natively supported by MongoDB in the same way.
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Configure Room entity
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => r.Number).IsUnique();
        });
    }
}




