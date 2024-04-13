using StartupNNTM.ViewModels;

namespace StartupNNTM.Service
{
    public interface ISendMailService
    {
        Task SendMail(MailContent mailContent);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
