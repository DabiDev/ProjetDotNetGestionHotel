using HotelManagement.Models;
using HotelManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data;

public static class SeedData
{
    public static async Task InitializeAsync(HotelDbContext context)
    {
        // Check if data already exists
        // Check if data already exists (checking Users is sufficient)
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var authService = new AuthService(context);

        // Create default Réceptionniste
        await authService.RegisterAsync(
            "Admin Réception",
            "reception@hotel.com",
            "reception123",
            "Réceptionniste"
        );

        // Create sample Client
        await authService.RegisterAsync(
            "Client Test",
            "client@test.com",
            "client123",
            "Client"
        );

        // Create sample rooms
        var rooms = new List<Room>
        {
            new Room { Number = "101", Type = "Simple", Capacity = 1, PricePerNight = 80.00m, IsActive = true },
            new Room { Number = "102", Type = "Double", Capacity = 2, PricePerNight = 120.00m, IsActive = true },
            new Room { Number = "103", Type = "Double", Capacity = 2, PricePerNight = 120.00m, IsActive = true },
            new Room { Number = "201", Type = "Suite", Capacity = 3, PricePerNight = 200.00m, IsActive = true },
            new Room { Number = "202", Type = "Suite", Capacity = 4, PricePerNight = 250.00m, IsActive = true },
            new Room { Number = "301", Type = "Simple", Capacity = 1, PricePerNight = 90.00m, IsActive = true },
            new Room { Number = "302", Type = "Double", Capacity = 2, PricePerNight = 130.00m, IsActive = true },
            new Room { Number = "303", Type = "Double", Capacity = 2, PricePerNight = 130.00m, IsActive = true }
        };

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
    }
}

