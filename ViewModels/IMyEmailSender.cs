namespace NIA_CRM.ViewModels
{
    public interface IMyEmailSender
    {
        Task SendOneAsync(string name, string email, string subject, string htmlMessage);
        Task SendToManyAsync(EmailMessage emailMessage);
    }
}
