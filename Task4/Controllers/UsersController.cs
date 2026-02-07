using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task4.Data;
using Task4.Models;

public class UsersController : Controller
{
    private readonly ApplicationDbContext db;

    public UsersController(ApplicationDbContext context)
    {
        db = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 15;

        var totalUsers = await db.Users.CountAsync();

        var users = await db.Users
            .OrderByDescending(x => x.LastLoginAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Block(Guid[] userIds)
    {
        if (userIds == null || userIds.Length == 0)
            return RedirectToAction(nameof(Index));

        var users = await db.Users
            .Where(x => userIds.Contains(x.Id))
            .ToListAsync();

        foreach (var u in users)
            u.Status = UserStatus.Blocked;

        await db.SaveChangesAsync();

        TempData["msg"] = "Users blocked";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(Guid[] userIds)
    {
        if (userIds == null || userIds.Length == 0)
            return RedirectToAction(nameof(Index));

        var users = await db.Users
            .Where(x => userIds.Contains(x.Id))
            .ToListAsync();

        foreach (var u in users)
            if (u.Status == UserStatus.Blocked)
                u.Status = UserStatus.Active;

        await db.SaveChangesAsync();

        TempData["msg"] = "Users unblocked";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid[] userIds)
    {
        if (userIds == null || userIds.Length == 0)
            return RedirectToAction(nameof(Index));

        var users = await db.Users
            .Where(x => userIds.Contains(x.Id))
            .ToListAsync();

        db.Users.RemoveRange(users);
        await db.SaveChangesAsync();

        TempData["msg"] = "Users deleted";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUnverified()
    {
        var users = await db.Users
            .Where(x => x.Status == UserStatus.Unverified)
            .ToListAsync();

        db.Users.RemoveRange(users);
        await db.SaveChangesAsync();

        TempData["msg"] = "Unverified users deleted";
        return RedirectToAction(nameof(Index));
    }
}