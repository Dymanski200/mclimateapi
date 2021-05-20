using MimeKit;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace WebAPI
{
    public class EmailService
    {
        public static IConfiguration Configuration { get; set; }

        public static string Name
        {
            get { return Configuration.GetValue<string>("EmailConfiguration:Name"); }
            set { Configuration["EmailConfiguration:Name"] = value; }
        }
        public static string Address
        {
            get { return Configuration.GetValue<string>("EmailConfiguration:Address"); }
            set { Configuration["EmailConfiguration:Address"] = value; }
        }
        public static string Password
        {
            get { return Configuration.GetValue<string>("EmailConfiguration:Password"); }
            set { Configuration["EmailConfiguration:Password"] = value; }
        }

        public static async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("MClimate "+Name, Address));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.yandex.ru", 25, false);
                await client.AuthenticateAsync(Address, Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
