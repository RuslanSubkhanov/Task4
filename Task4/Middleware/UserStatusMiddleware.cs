using Microsoft.EntityFrameworkCore;
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
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var email = context.User.Identity.Name;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.Status == UserStatus.Blocked)
            {
                context.Response.Redirect("/Account/Login");
                return;
            }
        }

        await _next(context);
    }
}