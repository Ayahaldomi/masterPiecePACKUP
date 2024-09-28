using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MailKit.Net.Smtp;
using MimeKit;

namespace MasterPiece.Controllers
{
    public class UserController : Controller
    {
        private MasterPieceEntities db = new MasterPieceEntities();

        // GET: User
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(Patient p)
        {
            var user = db.Patients.Where(l => l.Patient_ID == p.Patient_ID && l.Phone_Number == p.Phone_Number).FirstOrDefault();
            if (user == null) {
                return View();
            }
            Session["userSession"] = user;
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public ActionResult SignUp(Patient patient)
        {
            db.Patients.Add(patient);
            db.SaveChanges();
            try
            {
                string fromEmail = "election2024jordan@gmail.com";
                string fromName = "PrimeLab";
                string subjectText = "Patient ID";
                string messageText = $@"
                    <html>
                    <body>
                        <h2>Hello</h2>
                        <p>Welcome To PrimeLab</p>
                        <p>Your Paient ID is: {patient.Patient_ID}</p>
                        <p>With best regards,<br>Admin</p>
                    </body>
                    </html>";
                string toEmail = patient.Email;
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 465; // Port 465 for SSL

                string smtpUsername = "election2024jordan@gmail.com";
                string smtpPassword = "zwht jwiz ivfr viyt"; // Ensure this is correct

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subjectText;
                message.Body = new TextPart("html") { Text = messageText };

                using (var client = new SmtpClient())
                {
                    client.Connect(smtpServer, smtpPort, true); // Use SSL
                    client.Authenticate(smtpUsername, smtpPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
                TempData["Email"] = "An Email With Your Patient ID Was Sent To You!";
                return View("Login");
            }
            catch { 
                return View("Login");
            }

        }
        public ActionResult LogOut() {
            Session.Remove("userSession");
            return RedirectToAction("Index", "Home");
        }


        ////////////////////////////////////////////// Profile /////////////////////////////////////////////////
        public ActionResult Profile()
        {
            var s = Session["userSession"] as Patient;
            if (s == null)
            {

                return RedirectToAction("Index", "Home");
            }
            var r = db.Patients.Find(s.Patient_ID);
            
            Session["userSession"] = r;
            var user = db.Test_Order.Where(l => l.Patient_ID == s.Patient_ID).ToList();
            return View(user);
        }
        //////////////////////////////////////////////////// Profile Details  /////////////////////////////////////////////////
        public ActionResult ProfileEdit(int id)
        {
            var patient = db.Patients.FirstOrDefault(l => l.Patient_ID == id);

            return View(patient);
        }


        [HttpPost]
        public ActionResult ProfileEdit(Patient patient)
        {
            db.Entry(patient).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Profile");

        }
    }
}