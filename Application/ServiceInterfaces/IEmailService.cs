using System.Threading.Tasks;

namespace Application.ServiceInterfaces
{
    public interface IEmailService
    {
        void Configure(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}