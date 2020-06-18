using Microsoft.Extensions.Configuration;
using RESTAPI.Util;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RESTAPI.Mailing
{
    public class MailService : IMailService
    {
        private readonly string address;
        private readonly int port;
        private readonly NetworkCredential credential;

        public MailService(IConfiguration configuration)
        {
            var confSec = configuration.GetSection("MailService");

            address = confSec.GetValue<string>("Address");
            port = confSec.GetValue<int>("Port", 587);

            credential = new NetworkCredential()
            {
                UserName = confSec.GetValue<string>("Username"),
                SecurePassword = SecureStringUtil.FromString(confSec.GetValue<string>("Password")),
            };
        }

        public Task SendMailAsync(string fromDisplayName, string to, string subject, string body, bool isBodyHtml = false) =>
            SendMailAsync(credential.UserName, fromDisplayName, to, subject, body, isBodyHtml);

        public Task SendMailAsync(string from, string fromDisplayName, string to, string subject, string body, bool isBodyHtml = false)
        {
            var mail = new MailMessage()
            {
                From = new MailAddress(from, fromDisplayName),
                Subject = subject,
                IsBodyHtml = isBodyHtml,
                Body = body,
            };

            mail.To.Add(to);

            return SendMailRawAsync(mail);
        }

        public Task SendMailRawAsync(MailMessage mail) =>
            Task.Run(() => GetClient().Send(mail));

        private SmtpClient GetClient() =>
            new SmtpClient()
            {
                Host = address,
                Port = port,
                UseDefaultCredentials = false,
                Credentials = credential,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
            };
    }
}
