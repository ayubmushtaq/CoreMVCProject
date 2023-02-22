using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMVCProject.CommonHelper
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var toEmail = new MimeMessage();
            toEmail.From.Add(MailboxAddress.Parse("ayubmushtaqoffice1@gmail.com"));
            toEmail.To.Add(MailboxAddress.Parse(email));
            toEmail.Subject = subject;
            toEmail.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("ayubmushtaqoffice1@gmail.com", "Ayub1!2@3#");
                emailClient.SendAsync(toEmail);
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}
