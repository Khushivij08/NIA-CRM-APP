﻿namespace NIA_CRM.ViewModels
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpFromName { get; set; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }
    }
}
