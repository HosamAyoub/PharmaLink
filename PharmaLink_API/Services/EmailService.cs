using PharmaLink_API.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace PharmaLink_API.Services
{
    public class EmailService : IEmailService
    {
        public EmailService()
        {
            
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("apdom785@gmail.com", "12182381482"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage("apdom785@gmail.com", toEmail, subject, body);
            mailMessage.IsBodyHtml = true;
            await client.SendMailAsync(mailMessage);
        }

    }
}
