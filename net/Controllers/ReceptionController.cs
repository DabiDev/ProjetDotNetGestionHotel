using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ReceptionOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString("Role");
        if (role != "Réceptionniste")
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
        base.OnActionExecuting(context);
    }
}

[ReceptionOnly]
public class ReceptionController : Controller
{
    private readonly HotelDbContext _context;
    private readonly IReservationService _reservationService;

    public ReceptionController(HotelDbContext context, IReservationService reservationService)
    {
        _context = context;
        _reservationService = reservationService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var arrivals = await _reservationService.GetTodayArrivalsAsync();
        var departures = await _reservationService.GetTodayDeparturesAsync();
        
        var totalRooms = await _reservationService.GetTotalRoomsCountAsync();
        var occupationRate = await _reservationService.GetOccupationRateAsync();
        var occupiedRooms = (int)Math.Round((occupationRate / 100) * totalRooms);

        var viewModel = new DashboardViewModel
        {
            ArrivalsToday = arrivals,
            DeparturesToday = departures,
            TotalRooms = totalRooms,
            OccupiedRooms = occupiedRooms,
            OccupationRate = occupationRate
        };

        return View(viewModel);
    }

    // Rooms Management
    public async Task<IActionResult> Rooms()
    {
        var rooms = await _context.Rooms
            .OrderBy(r => r.Number)
            .ToListAsync();
        return View(rooms);
    }

    [HttpGet]
    public IActionResult CreateRoom()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(Room room)
    {
        if (ModelState.IsValid)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Chambre créée avec succès";
            return RedirectToAction("Rooms");
        }
        return View(room);
    }

    [HttpGet]
    public async Task<IActionResult> EditRoom(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoom(Room room)
    {
        if (ModelState.IsValid)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Chambre modifiée avec succès";
            return RedirectToAction("Rooms");
        }
        return View(room);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            TempData["Error"] = "Chambre introuvable";
            return RedirectToAction("Rooms");
        }

        // Check if room has active reservations
        var hasActiveReservations = await _context.Reservations
            .AnyAsync(r => r.RoomId == id && 
                          r.Status != ReservationStatus.Annulee && 
                          r.CheckOut > DateTime.Today);

        if (hasActiveReservations)
        {
            TempData["Error"] = "Impossible de supprimer une chambre avec des réservations actives";
            return RedirectToAction("Rooms");
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Chambre supprimée avec succès";
        return RedirectToAction("Rooms");
    }

    // Reservations Management
    public async Task<IActionResult> Reservations()
    {
        var reservations = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Room)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> EditReservation(Guid id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reservation == null)
        {
            return NotFound();
        }
        
        return View(reservation);
    }

    [HttpPost]
    public async Task<IActionResult> EditReservation(Guid id, ReservationStatus status, DateTime? checkIn = null, DateTime? checkOut = null)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reservation == null)
        {
            TempData["Error"] = "Réservation introuvable";
            return RedirectToAction("Reservations");
        }

        // Update dates if provided
        if (checkIn.HasValue && checkOut.HasValue)
        {
            if (checkOut.Value <= checkIn.Value)
            {
                TempData["Error"] = "La date de départ doit être postérieure à la date d'arrivée";
                return RedirectToAction("EditReservation", new { id });
            }

            // Check availability (excluding this reservation)
            var isAvailable = await _reservationService.IsRoomAvailableAsync(
                reservation.RoomId, 
                checkIn.Value, 
                checkOut.Value, 
                reservation.Id);

            if (!isAvailable)
            {
                TempData["Error"] = "La chambre n'est pas disponible pour ces dates";
                return RedirectToAction("EditReservation", new { id });
            }

            reservation.CheckIn = checkIn.Value;
            reservation.CheckOut = checkOut.Value;
            
            // Recalculate total
            var nights = (checkOut.Value - checkIn.Value).Days;
            reservation.Total = reservation.Room.PricePerNight * nights;
        }

        reservation.Status = status;
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Réservation modifiée avec succès";
        return RedirectToAction("Reservations");
    }

    [HttpPost]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            TempData["Error"] = "Réservation introuvable";
            return RedirectToAction("Reservations");
        }

        reservation.Status = ReservationStatus.Annulee;
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Réservation annulée avec succès";
        return RedirectToAction("Reservations");
    }

    [HttpPost]
    public async Task<IActionResult> ApproveReservation(Guid id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            TempData["Error"] = "Réservation introuvable";
            return RedirectToAction("Reservations");
        }

        if (reservation.Status != ReservationStatus.EnAttente)
        {
            TempData["Error"] = "Seules les réservations en attente peuvent être approuvées";
            return RedirectToAction("Reservations");
        }

        reservation.Status = ReservationStatus.Confirmee;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Réservation approuvée avec succès";
        return RedirectToAction("Reservations");
    }
}




