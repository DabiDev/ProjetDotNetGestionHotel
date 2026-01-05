using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models;

public class SearchViewModel
{
    [Required(ErrorMessage = "La date d'arrivée est requise")]
    [DataType(DataType.Date)]
    [Display(Name = "Date d'arrivée")]
    public DateTime CheckIn { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La date de départ est requise")]
    [DataType(DataType.Date)]
    [Display(Name = "Date de départ")]
    public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);

    public List<Room>? AvailableRooms { get; set; }
}




