using MasterPiece.Models;
using MasterPiece.PayPal;
using MasterPiece.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using Microsoft.AspNet.SignalR.Hubs;
using MimeKit;
using PayPal.Api;
using MailKit.Net.Smtp;

namespace MasterPiece.Controllers
{
    public class HomeController : Controller
    {
        private MasterPieceEntities db = new MasterPieceEntities();



        //////////////////////////////////////////////   Home  //////////////////////////////////////////////////

        public ActionResult Index()
        {
            var home = new HomeViewModel
            {
                Package = db.Packages.ToList(),
                Feedback = db.Feedbacks.Where(f => f.Status == "Approved").ToList(),
            };
            return View(home);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        /////////////////////////////////////////////////// Contact ///////////////////////////////////////////////////
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult contactPost(Contact contact)
        {
            contact.sent_date = DateTime.Now;
            contact.status = 0;
            db.Contacts.Add(contact);
            db.SaveChanges();
            TempData["confirm"] = "Your Message Was Sent Successfully";
            return View("Contact");
        }


        ///////////////////////////////////////////////// Packages ////////////////////////////////////////////////
        public ActionResult Service()
        {
            var home = new HomeViewModel
            {
                Package = db.Packages.ToList(),
                Feedback = db.Feedbacks.Where(f => f.Status == "Approved").ToList(),
            };
            return View(home);
        }

        //////////////////////////////////////////////////  Package Details  /////////////////////////////////////////////

        public ActionResult ServiceDetails(int id)
        {
            var packages = db.Packages.Include(p => p.Package_Tests.Select(pt => pt.Test)).FirstOrDefault(s => s.Package_ID == id);
            ViewBag.TestsList = db.Tests.ToList();
            return View(packages);

           
        }

        public ActionResult AppointmentDetails(int id)
        {
            var packageTests = db.Package_Tests.Where(t => t.Package_ID == id).ToList();
            var package = db.Packages.Find(id);
            List<Test> testsList = new List<Test>();

            // Iterate over each Package_Test and retrieve the corresponding Test
            foreach (var packageTest in packageTests)
            {
                var test = db.Tests.Where(t => t.Test_ID == packageTest.Test_ID).FirstOrDefault();
                if (test != null)
                {
                    testsList.Add(test);
                }
            }

            // Store the testsList in TempData
            TempData["TestsPackageList"] = testsList;
            TempData["PackagePrice"] = package.Price;

            return RedirectToAction("Appointment");
        }
        /////////////////////////////////////////////   Appointment   ///////////////////////////////////////////

        public ActionResult Appointment()
        {
            ViewBag.Message = "Your contact page.";
            var tests = db.Tests.ToList();
            return View(tests);
        }


        [HttpPost]
        public ActionResult ProcessPayment(int patientId, decimal value)
        {
            var patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                return HttpNotFound();
            }

            // Define redirect URLs
            string redirectUrl = Url.Action("PaymentSuccess", "Home", new { patientId }, protocol: Request.Url.Scheme);
            string cancelUrl = Url.Action("PaymentCancel", "Home", new { patientId }, protocol: Request.Url.Scheme);

            // Create PayPal payment
            var payment = PayPalHelper.CreatePayment(redirectUrl, cancelUrl, value); // Amount can be dynamic

            // Get the PayPal redirect URL and redirect the user
            var redirect = payment.links.FirstOrDefault(link => link.rel.ToLower().Trim().Equals("approval_url"));
            if (redirect == null)
            {
                return View("Error");
            }

            return Redirect(redirect.href);
        }

        // PayPal redirects here after the payment is approved by the user
        public ActionResult PaymentSuccess(string paymentId, string token, string PayerID, int patientId)
        {
            var executedPayment = PayPalHelper.ExecutePayment(paymentId, PayerID);

            if (executedPayment.state.ToLower() != "approved")
            {
                return View("Error");
            }
            // Payment is successful, retrieve the appointment data from session
            var appointmentData = Session["AppointmentData"] as AppointmentPOST;


            // Store the appointment data in TempData for the next action
            TempData["AppointmentData"] = appointmentData;

            // Proceed with creating the appointment
            return RedirectToAction("CreateAppointmentAfterPayment");
        
        }

        
        public ActionResult CreateAppointmentAfterPayment()
        {
            // Retrieve the appointment data from TempData
            var appointment = TempData["AppointmentData"] as AppointmentPOST;
            // Save appointment information (same as before)
            var app = new Appointment
            {
                Full_Name = appointment.Full_Name,
                Gender = appointment.Gender,
                Date_Of_Birth = appointment.Date_Of_Birth,
                Email_Address = appointment.Email_Address,
                Phone_Number = appointment.Phone_Number,
                Home_Address = appointment.Home_Address,
                Date_Of_Appo = appointment.Date_Of_Appo,
                Total_price = appointment.Total_price,
                Amount_paid = appointment.Amount_paid,
                Billing_ID = 2656,
                Status = "Pending" // Example status
            };
            db.Appointments.Add(app);
            db.SaveChanges();

            // Save selected tests
            foreach (var selectedTest in appointment.SelectedTests)
            {
                var appointmentTest = new Appointments_Tests
                {
                    Appointment_ID = app.ID, // Use the saved appointment ID
                    Test_ID = selectedTest.Test_ID
                };

                db.Appointments_Tests.Add(appointmentTest);
            }

            db.SaveChanges();

            try
            {
                string selectedTestsList = "";  
                foreach (var selectedTest in appointment.SelectedTests)
                {
                    var test = db.Tests.Where(p => p.Test_ID == selectedTest.Test_ID).FirstOrDefault();

                    selectedTestsList += test.Test_Name + ", ";
                }
                string appointmentDate = appointment.Date_Of_Appo.HasValue
                ? appointment.Date_Of_Appo.Value.ToString("MMMM dd, yyyy")
                : "No appointment date set";

                string fromEmail = "election2024jordan@gmail.com";
                string fromName = "PrimeLab";
                string subjectText = "Appointment";
                string messageText = $@"
            <html>
            <body>
                <h2>Hello {appointment.Full_Name},</h2>
                <p>Thank you for scheduling your appointment with PrimeLab.</p>
                <p><strong>Appointment Details:</strong></p>
                <ul>
                    <li><strong>Date of Appointment:</strong> {appointmentDate}</li>
                    <li><strong>Total Price:</strong> {appointment.Total_price:C}</li>
                    <li><strong>Amount Paid:</strong> {appointment.Amount_paid:C}</li>
                    <li><strong>Tests Scheduled:</strong> {selectedTestsList}</li>
                </ul>
                <p>If you have any questions or need to make changes to your appointment, feel free to contact us at this email or call us at (xxx-xxx-xxxx).</p>
                <p>We look forward to seeing you at PrimeLab!</p>
                <p>With best regards,<br>PrimeLab Team</p>
            </body>
            </html>";
                string toEmail = appointment.Email_Address;
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
                TempData["confirmForm"] = appointment.Full_Name;

                // Clear the appointment data from the session
                Session.Remove("AppointmentData");

                return RedirectToAction("Appointment");
            }
            catch
            {
                TempData["confirmForm"] = appointment.Full_Name;

                return RedirectToAction("Appointment");
            }
            
        }

        public ActionResult CloseTab() { return View(); }

        // Payment was canceled
        public ActionResult PaymentCancel(int patientId)
        {
            // Handle canceled payment logic here
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult StoreAppointmentData(AppointmentPOST appointment)
        {
            // Temporarily store the appointment data in the session (or you can use a database)
            Session["AppointmentData"] = appointment;

            return Json(new { success = true });
        }



        [HttpPost]
        public ActionResult CreateAppointment(AppointmentPOST appointment)
        {
            // Save appointment information
            var app = new Appointment
            {
                Full_Name = appointment.Full_Name,
                Gender = appointment.Gender,
                Date_Of_Birth = appointment.Date_Of_Birth,
                Email_Address = appointment.Email_Address,
                Phone_Number = appointment.Phone_Number,
                Home_Address = appointment.Home_Address,
                Date_Of_Appo = appointment.Date_Of_Appo,
                Total_price = appointment.Total_price,
                Amount_paid = appointment.Amount_paid,
                Billing_ID = 2656,
                Status = "Pending" // Example status
            };
            db.Appointments.Add(app);
            db.SaveChanges();

            // Save selected tests
            foreach (var selectedTest in appointment.SelectedTests)
            {
                var appointmentTest = new Appointments_Tests
                {
                    Appointment_ID = app.ID, 
                    Test_ID = selectedTest.Test_ID
                };

                db.Appointments_Tests.Add(appointmentTest);
            }

            db.SaveChanges();

            try
            {
                string selectedTestsList = "";  // Initialize an empty string to store the test names.

                foreach (var selectedTest in appointment.SelectedTests)
                {
                    var test = db.Tests.Where(p => p.Test_ID == selectedTest.Test_ID).FirstOrDefault();

                        selectedTestsList += test.Test_Name + ", ";
                }
                string appointmentDate = appointment.Date_Of_Appo.HasValue
                ? appointment.Date_Of_Appo.Value.ToString("MMMM dd, yyyy")
                : "No appointment date set";

                string fromEmail = "election2024jordan@gmail.com";
                string fromName = "PrimeLab";
                string subjectText = "Appointment";
                string messageText = $@"
            <html>
            <body>
                <h2>Hello {appointment.Full_Name},</h2>
                <p>Thank you for scheduling your appointment with PrimeLab.</p>
                <p><strong>Appointment Details:</strong></p>
                <ul>
                    <li><strong>Date of Appointment:</strong> {appointment.Date_Of_Appo}</li>
                    <li><strong>Total Price:</strong> {appointment.Total_price:C}</li>
                    <li><strong>Amount Paid:</strong> {appointment.Amount_paid:C}</li>
                    <li><strong>Tests Scheduled:</strong> {selectedTestsList}</li>
                </ul>
                <p>If you have any questions or need to make changes to your appointment, feel free to contact us at this email or call us at (xxx-xxx-xxxx).</p>
                <p>We look forward to seeing you at PrimeLab!</p>
                <p>With best regards,<br>PrimeLab Team</p>
            </body>
            </html>";
                string toEmail = appointment.Email_Address;
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
                TempData["confirmForm"] = appointment.Full_Name;

                return RedirectToAction("Appointment");
            }
            catch
            {
                TempData["confirmForm"] = appointment.Full_Name;

                return RedirectToAction("Appointment");
            }

           
        }

        [HttpGet]
        public JsonResult GetAvailableTimes(string date)
        {
            // Convert the string date to DateTime to match the Date_Of_Appo column
            DateTime selectedDate = DateTime.Parse(date);

            // Fetch appointments for the selected date (fetch all from DB first)
            var appointments = db.Appointments
                     .Where(a => a.Date_Of_Appo.HasValue)
                     .ToList(); // Bring the data into memory

            // Perform the date comparison in memory
            var bookedAppointments = appointments
                         .Where(a => a.Date_Of_Appo.Value.Date == selectedDate.Date) // Compare the date part only
                         .Select(a => a.Date_Of_Appo.Value.TimeOfDay)
                         .ToList();

            // Return the booked times as a list of strings in the "HH:mm" format
            var bookedTimes = bookedAppointments.Select(t => t.ToString(@"hh\:mm")).ToList();

            return Json(bookedTimes, JsonRequestBehavior.AllowGet);
        }
        //////////////////////////////////////////////////  Employee Portal  //////////////////////////////////////////////////////
        public ActionResult EmployeePortal()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult EmployeePortal(string Email, string Password)
        {
            var employee = db.Lab_Tech.Where(e => e.Email == Email && e.Password == Password).FirstOrDefault();
            if (employee == null) {
                TempData["LoginError"] = "Invalid login credentials.";
                return RedirectToAction("EmployeePortal");
            }
            if (employee.Status == "Doctor") {
                Session["Employee"] = employee;
                return RedirectToAction("DoctorDashboard", "Admin");
            }
            else
            {
                Session["Employee"] = employee;
                return RedirectToAction("AdminDashboard", "Admin");
            }
            

        }

    }
}