using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task4.Data;
using Task4.Models;
using Task4.ViewModels;

namespace Task4.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null || user.PasswordHash != model.Password || user.Status == UserStatus.Blocked)
        {
            ModelState.AddModelError("", "Invalid credentials");
            return View(model);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var claims = new[] { new Claim(ClaimTypes.Name, user.Email) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Users");
    }

    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            PasswordHash = model.Password,
            EmailConfirmToken = Guid.NewGuid().ToString()
        };

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Email already exists");
            return View(model);
        }

        var link = Url.Action(
            "ConfirmEmail",
            "Account",
            new { token = user.EmailConfirmToken },
            Request.Scheme
        );

        _ = Services.EmailService.SendAsync(user.Email, link!);

        return RedirectToAction("Login");
    }

    public async Task<IActionResult> ConfirmEmail(string token)
    {
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.EmailConfirmToken == token);

        if (user == null)
            return RedirectToAction("Login");

        if (user.Status != UserStatus.Blocked)
            user.Status = UserStatus.Active;

        user.EmailConfirmToken = null;
        await _db.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }
}
