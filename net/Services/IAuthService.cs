using HotelManagement.Models;

namespace HotelManagement.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> RegisterAsync(string name, string email, string password, string role = "Client");
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}




