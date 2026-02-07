using Task4.Data;
using Task4.Models;

namespace Task4.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var path = context.Request.Path.Value?.ToLower();

        // страницы без проверки
        if (path.StartsWith("/account/login") ||
            path.StartsWith("/account/register") ||
            path.StartsWith("/account/confirmemail"))
        {
            await _next(context);
            return;
        }

        var userId = context.Session.GetString("UserId");

        // не авторизован
        if (userId == null)
        {
            context.Response.Redirect("/Account/Login");
            return;
        }

        var user = await db.Users.FindAsync(Guid.Parse(userId));

        // пользователь удалён
        if (user == null)
        {
            context.Session.Clear();
            context.Response.Redirect("/Account/Login");
            return;
        }

        // пользователь заблокирован
        if (user.Status == UserStatus.Blocked)
        {
            context.Session.Clear();
            context.Response.Redirect("/Account/Login");
            return;
        }

        await _next(context);
    }
}