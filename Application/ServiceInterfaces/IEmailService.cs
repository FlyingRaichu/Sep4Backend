using System.Threading.Tasks;

namespace Application.ServiceInterfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}