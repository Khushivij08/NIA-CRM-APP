using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using NIA_CRM.Models;
using NIA_CRM.CustomControllers;

namespace NIA_CRM.Controllers
{
    public class EmailController : ElephantController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendEmail(EmailModal emailModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", emailModel);
            }

            try
            {
                using (var smtpClient = new SmtpClient("smtp.example.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential("your-email@example.com", "your-password");
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("your-email@example.com"),
                        Subject = emailModel.Subject,
                        Body = "This is a test email.",
                        IsBodyHtml = false
                    };
                    //mailMessage.To.Add(emailModel.EmailAddress);
                    smtpClient.Send(mailMessage);
                }
                ViewBag.Message = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }
            return View("Index", emailModel);
        }
    }
}
