using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace FirstWebApp.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IConfiguration _config;

        public ContactModel(IConfiguration config)
        {
            _config = config;
        }

        public bool hasData = false;
        public string firstName = "";
        public string lastName = "";
        public string message = "";
        public string email = "";
        public string phone = "";

        public void OnGet()
        {
            if (TempData.ContainsKey("FirstName"))
            {
                hasData = true;
                firstName = TempData["FirstName"]?.ToString();
                lastName = TempData["LastName"]?.ToString();
                message = TempData["Message"]?.ToString();
            }
        }


        public IActionResult OnPost()
        {
            firstName = Request.Form["firstName"];
            lastName = Request.Form["lastName"];
            message = Request.Form["message"];
            email = Request.Form["email"];
            phone = Request.Form["phone"];

            var smtpSection = _config.GetSection("Smtp");

            var smtpClient = new SmtpClient(smtpSection["Host"])
            {
                Port = int.Parse(smtpSection["Port"]),
                Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]),
                EnableSsl = bool.Parse(smtpSection["EnableSsl"])
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpSection["Username"]),
                Subject = $"New Contact Message from {firstName} {lastName}",
                Body = $"Email: {email}\nPhone: {phone}\n\nMessage:\n{message}"
            };

            mail.To.Add(smtpSection["Username"]); // Send to yourself

            try
            {
                smtpClient.Send(mail);

                // Store form data temporarily
                TempData["FirstName"] = firstName;
                TempData["LastName"] = lastName;
                TempData["Message"] = message;

                return RedirectToPage(); // <-- PRG: Prevent re-send on refresh
            }
            catch (Exception ex)
            {
                message = "Failed to send email: " + ex.Message;
                hasData = true;
                return Page(); // Return the current page with error message
            }
        }

    }
}
