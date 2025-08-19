
using PharmaLink_API.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace PharmaLink_API.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;

        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task sendEmailAsync(string email, string subject, string message)
        {
            var apiKey = configuration["EmailSettings:key"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(configuration["EmailSettings:fromEmail"], configuration["EmailSettings:fromName"]);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, message);
            var response = await client.SendEmailAsync(msg);
            Console.WriteLine(response.ToString());
        }
    }
}
