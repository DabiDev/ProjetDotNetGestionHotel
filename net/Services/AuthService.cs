using HotelManagement.Data;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class AuthService : IAuthService
{
    private readonly HotelDbContext _context;

    public AuthService(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    public async Task<User?> RegisterAsync(string name, string email, string password, string role = "Client")
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return null;
        }

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

