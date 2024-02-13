using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDF_project
{
    internal class EmailManager
    {
        public void sendEmails(List<string> emails, string fromMail, string fromPassword, string emailBody)
        {
            foreach (string email in emails)
            {
                // send email
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "jauns slepenais draudzins!";
                message.To.Add(new MailAddress(email));
                message.Body = $"<html><body><strong>All data from https://va.lvceli.lv/ has been collected and put in Google Sheets: </strong>{emailBody} </body></html>";
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
                Console.WriteLine($"Email to {email} has been sent!");
            }
        }
    }
}
