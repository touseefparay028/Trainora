using LearningManagementSystem.Models.DTO;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace LearningManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        public async Task SendMail(string TO, string Subject, string Body, bool ishtml = false)
        {
            //Getting sender information from the configuration
            string? SenderName = _configuration["EmailSetting:SenderName"];
            string? SenderEmailAddress = _configuration["EmailSetting:SenderEmail"];
            string? Server = _configuration["EmailSetting:SmtpServer"];
            int port = Convert.ToInt32(_configuration["EmailSetting:Port"]);
            string? password = _configuration["EmailSetting:Password"];

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(SenderName, SenderEmailAddress));
            email.To.Add(MailboxAddress.Parse(TO));
            email.Subject = Subject;
            email.Body = new TextPart("plain")
            {
                Text = Body
            };
            //to connect with smtp
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(Server, port, false);
                await client.AuthenticateAsync(SenderEmailAddress, password);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }
        }
    }
}
