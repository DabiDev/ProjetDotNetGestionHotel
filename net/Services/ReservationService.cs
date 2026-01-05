using HotelManagement.Data;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class ReservationService : IReservationService
{
    private readonly HotelDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public ReservationService(HotelDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
    {
        // Validate dates
        if (checkOut <= checkIn)
        {
            return new List<Room>();
        }

        // Get all active rooms
        var allRooms = await _context.Rooms
            .AsNoTracking()
            .Where(r => r.IsActive)
            .ToListAsync();

        // Filter rooms that have no overlapping reservations
        var availableRooms = new List<Room>();

        // Use a fresh context for the loop checks to avoid provider mismatch issues
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            foreach (var room in allRooms)
            {
                var hasOverlap = await scopedContext.Reservations
                    .AsNoTracking()
                    .Where(r => r.RoomId == room.Id &&
                               r.Status != ReservationStatus.Annulee &&
                               ((r.CheckIn < checkOut && r.CheckOut > checkIn)))
                    .AnyAsync();

                if (!hasOverlap)
                {
                    availableRooms.Add(room);
                }
            }
        }

        return availableRooms;
    }

    public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null)
    {
        if (checkOut <= checkIn)
        {
            return false;
        }

        // Use a fresh scope/context to avoid "TargetException: Object type MongoQueryProvider... does not match target type"
        // This generally happens when the context has loaded other entities and the provider internal state gets confused.
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            var query = scopedContext.Reservations.AsNoTracking();

            if (excludeReservationId.HasValue)
            {
                query = query.Where(r => r.Id != excludeReservationId.Value);
            }

            var hasOverlap = await query.AnyAsync(r => 
                r.RoomId == roomId &&
                r.Status != ReservationStatus.Annulee &&
                r.CheckIn < checkOut && 
                r.CheckOut > checkIn);

            return !hasOverlap;
        }
    }

    public async Task<Reservation?> CreateReservationAsync(Guid userId, Guid roomId, DateTime checkIn, DateTime checkOut)
    {
        // PHASE 1: READ ROOM (Scope A)
        // Strictly isolated to Room entity to avoid Provider Mismatch
        decimal pricePerNight;
        
        using (var roomScope = _serviceProvider.CreateScope())
        {
            var roomContext = roomScope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            // Checks...
            if (checkOut <= checkIn) return null;

            var room = await roomContext.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null || !room.IsActive) return null;
            
            pricePerNight = room.PricePerNight;
        }

        // PHASE 2: CHECK AVAILABILITY (Scope B)
        // Re-use IsRoomAvailableAsync which we already fixed to use its own isolated Scope.
        // This ensures the Reservation query happens in a completely fresh context.
        var isAvailable = await IsRoomAvailableAsync(roomId, checkIn, checkOut);
        if (!isAvailable)
        {
            return null;
        }

        // PHASE 3: WRITE RESERVATION (Scope C)
        // Strictly isolated to Reservation entity (Write)
        using (var writeScope = _serviceProvider.CreateScope())
        {
            var writeContext = writeScope.ServiceProvider.GetRequiredService<HotelDbContext>();

            // Calculate total
            var nights = (checkOut - checkIn).Days;
            var total = pricePerNight * nights;

            // Create reservation
            var reservation = new Reservation
            {
                UserId = userId,
                RoomId = roomId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Status = ReservationStatus.EnAttente,
                CreatedAt = DateTime.Now,
                Total = total
            };

            writeContext.Reservations.Add(reservation);
            await writeContext.SaveChangesAsync();

            return reservation;
        }
    }

    public async Task<List<Reservation>> GetTodayArrivalsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        // 1. Fetch Reservations without Includes to avoid provider errors
        var reservations = await _context.Reservations
            .Where(r => r.CheckIn >= today && 
                        r.CheckIn < tomorrow && 
                        r.Status != ReservationStatus.Annulee)
            .OrderBy(r => r.CheckIn)
            .ToListAsync();

        if (!reservations.Any()) return reservations;

        // 2. Extract IDs
        var userIds = reservations.Select(r => r.UserId).Distinct().ToList();
        var roomIds = reservations.Select(r => r.RoomId).Distinct().ToList();

        // 3. Fetch related entities
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var rooms = await _context.Rooms
            .Where(r => roomIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id);

        // 4. Stitch in memory
        foreach (var reservation in reservations)
        {
            if (users.TryGetValue(reservation.UserId, out var user))
                reservation.User = user;
            
            if (rooms.TryGetValue(reservation.RoomId, out var room))
                reservation.Room = room;
        }

        return reservations;
    }

    public async Task<List<Reservation>> GetTodayDeparturesAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        // 1. Fetch Reservations without Includes
        var reservations = await _context.Reservations
            .Where(r => r.CheckOut >= today && 
                        r.CheckOut < tomorrow && 
                        r.Status != ReservationStatus.Annulee)
            .OrderBy(r => r.CheckOut)
            .ToListAsync();

        if (!reservations.Any()) return reservations;

        // 2. Extract IDs
        var userIds = reservations.Select(r => r.UserId).Distinct().ToList();
        var roomIds = reservations.Select(r => r.RoomId).Distinct().ToList();

        // 3. Fetch related entities
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var rooms = await _context.Rooms
            .Where(r => roomIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id);

        // 4. Stitch in memory
        foreach (var reservation in reservations)
        {
            if (users.TryGetValue(reservation.UserId, out var user))
                reservation.User = user;
            
            if (rooms.TryGetValue(reservation.RoomId, out var room))
                reservation.Room = room;
        }

        return reservations;
    }
    public async Task<int> GetTotalRoomsCountAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            return await scopedContext.Rooms.AsNoTracking().Where(r => r.IsActive).CountAsync();
        }
    }

    public async Task<decimal> GetOccupationRateAsync()
    {
        int totalRooms;
        // Scope 1: Count Rooms
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            totalRooms = await context.Rooms.AsNoTracking().Where(r => r.IsActive).CountAsync();
        }

        if (totalRooms == 0) return 0;

        int occupiedRooms;
        // Scope 2: Count Reservations
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            var today = DateTime.Today;
            occupiedRooms = await context.Reservations
                .AsNoTracking()
                .Where(r => r.CheckIn <= today && 
                           r.CheckOut > today && 
                           r.Status != ReservationStatus.Annulee)
                .Select(r => r.RoomId)
                .Distinct()
                .CountAsync();
        }

        return (decimal)occupiedRooms / totalRooms * 100;
    }
}




