using System;
using SendGrid;
using System.Net;
using System.Configuration;
using System.Diagnostics;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using P4ViewProject.Models;
using System.Net.Mail;

namespace P4ViewProject.Infrastructure
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            await configSendGridasync(message);
        }

        // Use NuGet to install SendGrid (Basic C# client lib) 
        private async Task configSendGridasync(IdentityMessage message)
        {
            var myMessage = new SendGridMessage();
            myMessage.AddTo(message.Destination);
            myMessage.From = new MailAddress(
                                "niyazi_oz@hotmail.com", "Niyazi Ozturk");
            myMessage.Subject = message.Subject;
            myMessage.Text = message.Body;
            myMessage.Html = message.Body;

            var credentials = new NetworkCredential(
                       ConfigurationManager.AppSettings["mailAccount"],
                       ConfigurationManager.AppSettings["mailPassword"]
                       );

            // Create a Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            if (transportWeb != null)
            {
                await transportWeb.DeliverAsync(myMessage);
            }
            else
            {
                Trace.TraceError("Failed to create Web transport.");
                await Task.FromResult(0);
            }
        }

        public async Task SendEmailToAdmin(ApplicationUser user) {
            //MailMessage m = new MailMessage(
            //            new MailAddress("sender@mydomain.com", "Web Registration"),
            //            new MailAddress(user.Email)
            //    );
            //m.Subject = "Email confirmation";
            //m.Body = string.Format(" {0} named user is registered with the email <{1}> and waiting for approval to use the portal.",
            //       user.UserName, user.Email);
            //m.IsBodyHtml = true;
            //SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            //smtp.Port = 465;
            //smtp.Credentials = new NetworkCredential("niyaziozturkg@gmail.com", "gmail2007");
            //smtp.EnableSsl = true;
            //smtp.Send(m);

        }

    }

    public class SendEmailbyGoogle{
    
        public async Task SendMail(string emailTo, String message, string subject)
        {

        // System.Net.Mail.
        MailMessage m = new MailMessage( /* from , to */
                        new MailAddress("niyaziozturkg@gmail.com", "View Web Admin"),
                        new MailAddress(emailTo)
                        );

        m.Subject = subject;  //"Email confirmation";
        m.Body = message;
        //string.Format("Dear {0}<BR/>Thank you for your registration, please click on the below link to comlete your registration: <a href=\"{1}\" title=\"User Email Confirm\">{1}</a>", user.UserName, Url.Action("ConfirmEmail", "Account", new { Token = user.Id, Email = user.Email }, Request.Url.Scheme));
        m.IsBodyHtml = true;

        //System.Net.Mail.
        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        // TODO: 
        // Move the credentials to webconfig file and try like that
        // ***
        smtp.Credentials = new System.Net.NetworkCredential(
                   ConfigurationManager.AppSettings["mailAccount"],
                   ConfigurationManager.AppSettings["mailPassword"]);
        /*"niyaziozturkg@gmail.com", "gmail2007"); */
        smtp.EnableSsl = true;
        smtp.Send(m);
        }
    }
     
    
}