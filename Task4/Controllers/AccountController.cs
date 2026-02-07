using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task4.Models;
using System.Security.Cryptography;
using System.Text;
using Task4.Data;

public class AccountController : Controller
{
    private readonly ApplicationDbContext db;

    public AccountController(ApplicationDbContext context)
    {
        db = context;
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var hash = HashPassword(password);

        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Email == email && x.PasswordHash == hash);

        if (user == null)
        {
            ViewBag.Error = "Invalid email or password";
            return View();
        }

        if (user.Status == UserStatus.Blocked)
        {
            ViewBag.Error = "User is blocked";
            return View();
        }

        HttpContext.Session.SetString("UserId", user.Id.ToString());

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return RedirectToAction("Index", "Users");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Fill all fields";
            return View();
        }

        if (await db.Users.AnyAsync(x => x.Email == email))
        {
            ViewBag.Error = "Email already exists";
            return View();
        }

        var token = Guid.NewGuid().ToString();

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = HashPassword(password),
            Status = UserStatus.Unverified,
            EmailConfirmToken = token,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var confirmLink = Url.Action(
            "ConfirmEmail",
            "Account",
            new { userId = user.Id, token = token },
            Request.Scheme);

        ViewBag.Success = "Registration successful!";
        ViewBag.ConfirmLink = confirmLink;

        return View();
    }

    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(x =>
                x.Id == userId &&
                x.EmailConfirmToken == token);

        if (user == null)
            return RedirectToAction("Register");

        if (user.Status != UserStatus.Blocked)
            user.Status = UserStatus.Active;

        user.EmailConfirmToken = null;

        await db.SaveChangesAsync();

        TempData["msg"] = "Email verified successfully. You can login.";

        return RedirectToAction("Login");
    }
}