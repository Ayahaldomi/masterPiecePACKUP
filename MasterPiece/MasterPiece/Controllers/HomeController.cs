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

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

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

            // Update the patient's payment status to "Paid"
            //var patient = db.Patients.Find(patientId);
            //patient.PaymentStatus = "Paid";
            //db.SaveChanges();

            //// After successful payment, redirect to the chat room
            //var chatRoomId = db.ChatRooms.FirstOrDefault(cr => cr.Patient_ID == patientId)?.ChatRoom_ID;
            //if (chatRoomId.HasValue)
            //{
            //    return RedirectToAction("Chat2", "Chat", new { chatRoomId = chatRoomId.Value });
            //}

            //return RedirectToAction("Index", "Home");

            // Return a view that will close the tab
            return View("CloseTab");
        }

        public ActionResult CloseTab() { return View(); }

        // Payment was canceled
        public ActionResult PaymentCancel(int patientId)
        {
            // Handle canceled payment logic here
            return RedirectToAction("Index", "Home");
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
                    Appointment_ID = app.ID, // Use the saved appointment ID
                    Test_ID = selectedTest.Test_ID
                };

                db.Appointments_Tests.Add(appointmentTest);
            }

            db.SaveChanges();

            return RedirectToAction("Index");
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

        public ActionResult EmployeePortal()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}