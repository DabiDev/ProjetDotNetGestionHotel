using HotelManagement.Models;

namespace HotelManagement.Services;

public interface IReservationService
{
    Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
    Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null);
    Task<Reservation?> CreateReservationAsync(Guid userId, Guid roomId, DateTime checkIn, DateTime checkOut);
    Task<List<Reservation>> GetTodayArrivalsAsync();
    Task<List<Reservation>> GetTodayDeparturesAsync();
    Task<int> GetTotalRoomsCountAsync();
    Task<decimal> GetOccupationRateAsync();
}




