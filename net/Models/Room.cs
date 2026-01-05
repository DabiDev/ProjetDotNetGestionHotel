using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Models;

public class Room
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // Simple, Double, Suite, etc.

    [Required]
    [Range(1, 10)]
    public int Capacity { get; set; }

    [Required]
    public decimal PricePerNight { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}




