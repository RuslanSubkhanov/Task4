using System.Net;
using System.Net.Mail;

namespace Task4.Services;

public static class EmailService
{
    public static Task SendAsync(string to, string link)
    {
        Console.WriteLine($"EMAIL TO: {to}");
        Console.WriteLine($"CONFIRM LINK: {link}");
        return Task.CompletedTask;
    }

}
