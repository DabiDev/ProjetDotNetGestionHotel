using Microsoft.AspNetCore.Mvc;
using HotelManagement.Services;
using HotelManagement.Models;

namespace HotelManagement.Controllers;

public class HomeController : Controller
{
    private readonly IAuthService _authService;

    public HomeController(IAuthService authService)
    {
        _authService = authService;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId))
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Réceptionniste")
            {
                return RedirectToAction("Dashboard", "Reception");
            }
            else
            {
                return RedirectToAction("Search", "Client");
            }
        }
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Email et mot de passe requis";
            return View();
        }

        var user = await _authService.AuthenticateAsync(email, password);
        if (user == null)
        {
            ViewBag.Error = "Email ou mot de passe incorrect";
            return View();
        }

        // Store user info in session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("Role", user.Role);

        // Redirect based on role
        if (user.Role == "Réceptionniste")
        {
            return RedirectToAction("Dashboard", "Reception");
        }
        else
        {
            return RedirectToAction("Search", "Client");
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Tous les champs sont requis";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Les mots de passe ne correspondent pas";
            return View();
        }

        var user = await _authService.RegisterAsync(name, email, password, "Client");
        if (user == null)
        {
            ViewBag.Error = "Cet email est déjà utilisé";
            return View();
        }

        // Auto login after registration
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("Role", user.Role);

        return RedirectToAction("Search", "Client");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}




