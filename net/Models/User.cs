using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "Client"; // Client ou Réceptionniste

    // Navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}




