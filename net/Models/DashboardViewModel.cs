using HotelManagement.Models;

namespace HotelManagement.Models;

public class DashboardViewModel
{
    public List<Reservation> ArrivalsToday { get; set; } = new();
    public List<Reservation> DeparturesToday { get; set; } = new();
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public decimal OccupationRate { get; set; }
}




