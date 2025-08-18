
using PharmaLink_API.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace PharmaLink_API.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task sendEmailAsync(string email, string subject, string message)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("apdom765@gmail.com","PharmaLink");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, message);
            var response = await client.SendEmailAsync(msg);
            Console.WriteLine(response.ToString());
        }
    }
}
