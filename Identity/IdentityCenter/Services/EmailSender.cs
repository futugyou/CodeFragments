
using Microsoft.AspNetCore.Identity.UI.Services;

namespace IdentityCenter.Services;
public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine(email);
        return Task.CompletedTask;
    }
}
