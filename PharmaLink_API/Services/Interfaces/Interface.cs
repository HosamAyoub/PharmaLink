namespace PharmaLink_API.Services.Interfaces
{
    public interface IEmailSender
    {
        Task sendEmailAsync(string email, string subject, string message);
    }
}
