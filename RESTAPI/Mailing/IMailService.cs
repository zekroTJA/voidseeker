using System.Net.Mail;
using System.Threading.Tasks;

namespace RESTAPI.Mailing
{
    public interface IMailService
    {
        Task SendMailRawAsync(MailMessage mail);
        Task SendMailAsync(string fromDisplayName, string to, string subject, string body, bool isBodyHtml = false);
        Task SendMailAsync(string from, string fromDisplayName, string to, string subject, string body, bool isBodyHtml = false);
    }
}
