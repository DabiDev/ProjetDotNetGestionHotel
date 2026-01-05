using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.Services;

namespace HotelManagement.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ClientOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString("Role");
        if (role != "Client")
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
        base.OnActionExecuting(context);
    }
}

public class ClientController : Controller
{
    private readonly HotelDbContext _context;
    private readonly IReservationService _reservationService;

    public ClientController(HotelDbContext context, IReservationService reservationService)
    {
        _context = context;
        _reservationService = reservationService;
    }

    [ClientOnly]
    [HttpGet]
    public IActionResult Search()
    {
        return View(new SearchViewModel());
    }

    [ClientOnly]
    [HttpPost]
    public async Task<IActionResult> Search(SearchViewModel model)
    {
        if (model.CheckOut <= model.CheckIn)
        {
            ModelState.AddModelError("CheckOut", "La date de départ doit être postérieure à la date d'arrivée");
            return View(model);
        }

        model.AvailableRooms = await _reservationService.GetAvailableRoomsAsync(model.CheckIn, model.CheckOut);
        return View(model);
    }

    [ClientOnly]
    [HttpPost]
    public async Task<IActionResult> Book(Guid roomId, DateTime checkIn, DateTime checkOut)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "Home");
        }

        if (checkOut <= checkIn)
        {
            TempData["Error"] = "Les dates sont invalides";
            return RedirectToAction("Search");
        }

        var userId = Guid.Parse(userIdStr);
        var reservation = await _reservationService.CreateReservationAsync(userId, roomId, checkIn, checkOut);
        if (reservation == null)
        {
            TempData["Error"] = "La chambre n'est plus disponible pour ces dates";
            return RedirectToAction("Search");
        }

        TempData["Success"] = "Réservation créée avec succès";
        return RedirectToAction("MyReservations");
    }

    [ClientOnly]
    public async Task<IActionResult> MyReservations()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "Home");
        }

        var userId = Guid.Parse(userIdStr);
        var reservations = await _context.Reservations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        if (reservations.Any())
        {
            var roomIds = reservations.Select(r => r.RoomId).Distinct().ToList();
            var rooms = await _context.Rooms
                .Where(r => roomIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id);

            foreach (var reservation in reservations)
            {
                if (rooms.TryGetValue(reservation.RoomId, out var room))
                {
                    reservation.Room = room;
                }
            }
        }

        return View(reservations);
    }

    [ClientOnly]
    [HttpPost]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "Home");
        }

        var userId = Guid.Parse(userIdStr);
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null || reservation.UserId != userId)
        {
            TempData["Error"] = "Réservation introuvable";
            return RedirectToAction("MyReservations");
        }

        if (reservation.Status == ReservationStatus.Annulee)
        {
            TempData["Error"] = "Cette réservation est déjà annulée";
            return RedirectToAction("MyReservations");
        }

        reservation.Status = ReservationStatus.Annulee;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Réservation annulée avec succès";
        return RedirectToAction("MyReservations");
    }
}

