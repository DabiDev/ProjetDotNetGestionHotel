using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Models;

public enum ReservationStatus
{
    EnAttente = 0,
    Confirmee = 1,
    Annulee = 2,
    Terminee = 3
}

public class Reservation
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckIn { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckOut { get; set; }

    [Required]
    public ReservationStatus Status { get; set; } = ReservationStatus.EnAttente;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public decimal Total { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
}




