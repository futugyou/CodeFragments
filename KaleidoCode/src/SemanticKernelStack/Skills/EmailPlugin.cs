
using System.ComponentModel;

namespace SemanticKernelStack.Skills;

public class EmailPlugin
{
    private readonly IEmailService EmailService;
    public EmailPlugin(IEmailService emailService)
    {
        EmailService = emailService;
    }

    [KernelFunction]
    [Description("Sends an email to a recipient.")]
    public async Task SendEmailAsync(
        Kernel kernel,
        [Description("Semicolon delimitated list of emails of the recipients")] string recipientEmails,
        string subject,
        string body
    )
    {
        await EmailService.SendEmailAsync();
        // Add logic to send an email using the recipientEmails, subject, and body
        // For now, we'll just print out a success message to the console
        Console.WriteLine("Email sent!");
    }
}

public interface IEmailService
{
    Task SendEmailAsync();
}

public class EmailService : IEmailService
{
    public Task SendEmailAsync()
    {
        return Task.CompletedTask;
    }
}