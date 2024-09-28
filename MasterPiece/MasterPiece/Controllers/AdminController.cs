using MasterPiece.Models;
using MasterPiece.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Microsoft.AspNet.SignalR.Hubs;
using PayPal.Api;
using MimeKit;
using MailKit.Net.Smtp;

namespace MasterPiece.Controllers
{
    public class AdminController : Controller
    {
        private MasterPieceEntities db = new MasterPieceEntities();

        // GET: Admin
        public ActionResult AdminDashboard()
        {
            return View();
        }




        ///////////////////////////////////////////////  Patient  ///////////////////////////////////////////////////

        public ActionResult AddPatient()
        {
            var patient = db.Patients.ToList();
            return View(patient);
        }


        [HttpPost]
        public ActionResult AddPatient(Patient patient) 
        {
            if (patient.Patient_ID == 0 || patient.Patient_ID == null)
            {
                db.Patients.Add(patient);

                var order = new Test_Order
                {
                    Patient_ID = patient.Patient_ID,
                    Date = DateTime.Now,
                    Tech_ID = 1,
                    Status = "Pending"

                };
                db.Test_Order.Add(order);
                db.SaveChanges();
                return RedirectToAction("AddPatientTests", new {orderID = order.Order_ID});
            }
            else
            {
                var existPatient = db.Patients.FirstOrDefault(p => p.Patient_ID == patient.Patient_ID);
                existPatient.Full_Name = patient.Full_Name;
                existPatient.Date_Of_Birth = patient.Date_Of_Birth;
                existPatient.Gender = patient.Gender;
                existPatient.Marital_Status = patient.Marital_Status;
                existPatient.Nationality = patient.Nationality;
                existPatient.Phone_Number = patient.Phone_Number;
                existPatient.Home_Address = patient.Home_Address;
                existPatient.Note = patient.Note;

                db.Entry(existPatient).State = EntityState.Modified;
                
                var order = new Test_Order
                {
                    Patient_ID = patient.Patient_ID,
                    Date = DateTime.Now,
                    Tech_ID = 1,
                    Status = "Pending"

                };
                db.Test_Order.Add(order);
                db.SaveChanges();

                return RedirectToAction("AddPatientTests", new { orderID = order.Order_ID });

            }
        }

        public ActionResult AddPatientTests(int orderID)//int orderID
        {
            var order = db.Test_Order.Find(orderID);
            ViewBag.TestsList = db.Tests.ToList();
            return View(order);
        }


        [HttpPost]
        public ActionResult SaveTests(int orderId, List<Test_Order_Tests> selectedTests)
        {
            // Get the current order
            var order = db.Test_Order.FirstOrDefault(o => o.Order_ID == orderId);
            if (order == null)
            {
                return HttpNotFound("Order not found.");
            }

            if (order.Test_Order_Tests != null && order.Test_Order_Tests.Any())
            {
                db.Test_Order_Tests.RemoveRange(order.Test_Order_Tests);
            }


            decimal totalPrice = 0;
            foreach (var testId in selectedTests)
            {
                var test = db.Tests.FirstOrDefault(t => t.Test_ID == testId.Test_ID);
                if (test != null)
                {
                    var orderTest = new Test_Order_Tests
                    {
                        Order_ID = orderId,
                        Test_ID = testId.Test_ID,
                        Date_Of_Result = null,  
                        Result = null,          
                        Comment = null,
                        Status = "Pending"      
                    };
                    db.Test_Order_Tests.Add(orderTest);

                    // Add the price of the test to the total
                    totalPrice += test.Price ?? 0;
                }
            }

            // Update the total price of the order
            order.Total_Price = totalPrice;
            db.Entry(order).State = EntityState.Modified;

            // Save all changes to the database
            db.SaveChanges();

            // Redirect to the payment page (or another appropriate page)
            return RedirectToAction("AddPatientPayment", new { orderId = order.Order_ID });
        }


        public ActionResult AddPatientPayment(int orderId) //int orderId
        {
            var order = db.Test_Order.Find(orderId);
            return View(order);
        }

        [HttpPost]
        public ActionResult AddPatientPayment(Test_Order model) 
        { 
            var Order = db.Test_Order.Find(model.Order_ID);

            if (model.Discount_Persent == null)
            {
                Order.Discount_Persent = 0;
            }
            else
            {
                Order.Discount_Persent = model.Discount_Persent;
            }
            if (Order.Amount_Paid == null)
            {
                Order.Amount_Paid = 0;
            }
            Order.Amount_Paid += model.Amount_Paid;
            db.Entry(Order).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("AddPatientPayment", new { orderId  = model.Order_ID });//model.Order_ID


        }

        /////////////////////////////////////////// Manage Patient //////////////////////////////////////////////

        public ActionResult ManagePatient()
        {
            var patients = db.Patients.OrderByDescending(p => p.Patient_ID).ToList();
            return View(patients);
        }

        public ActionResult ManagePatientDetails(int patientID)
        {
            var patient = new PatientAndTests
            {
                Patients = db.Patients.Find(patientID),
                TestOrders = db.Test_Order.Where(p => p.Patient_ID == patientID).ToList()
            };
            return View(patient);
        }

        [HttpPost]
        public ActionResult ManagePatientDetails(Patient patient)
        {
            db.Entry(patient).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ManagePatientDetails", new { patientID = patient.Patient_ID });
        }
        //////////////////////////////////////////   Test Result  ///////////////////////////////////////////////////
        public ActionResult TestResults()
        {
            var tests = db.Test_Order.OrderByDescending(t =>  t.Patient_ID).ToList();

            return View(tests);
        }

        public ActionResult TestResultsAdd(int OrderID)// int OrderID
        {
            var order = db.Test_Order.Find(OrderID);
            return View(order);
        }

        public ActionResult Status(int Orderid) 
        {
            var order = db.Test_Order.Find(Orderid);
            order.Status = "Completed";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("TestResultsAdd", new { OrderID = Orderid });

        }

        [HttpPost]
        public ActionResult SaveTestResults(Test_Order model)
        {
            if (ModelState.IsValid)
            {
                foreach (var testResult in model.Test_Order_Tests)
                {
                    var existingTestResult = db.Test_Order_Tests.FirstOrDefault(t => t.ID == testResult.ID);
                    if (testResult.Result != null)
                    {
                        existingTestResult.Result = testResult.Result;
                        existingTestResult.Status = "Completed";
                        db.Entry(existingTestResult).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();

                return RedirectToAction("TestResultsAdd", new { Orderid = model.Order_ID });
            }

            return View(model); // Return view with model if there's an error
        }

        


        ///////////////////////////////////////  Appointment  ////////////////////////////////////////////////////
        public ActionResult Appointment()
        {
            var appointments = from a in db.Appointments
                               select new AppointmentViewModel
                               {
                                   ID = a.ID,
                                   Full_Name = a.Full_Name,
                                   Gender = a.Gender,
                                   Date_Of_Birth = a.Date_Of_Birth,
                                   Email_Address = a.Email_Address,
                                   Phone_Number = a.Phone_Number,
                                   Home_Address = a.Home_Address,
                                   Date_Of_Appo = a.Date_Of_Appo,
                                   Total_price = a.Total_price,
                                   Amount_paid = a.Amount_paid,
                                   Billing_ID = a.Billing_ID,
                                   Status = a.Status,

                                   // Query to get the test names for each appointment
                                   TestNames = (from at in db.Appointments_Tests
                                                join t in db.Tests on at.Test_ID equals t.Test_ID
                                                where at.Appointment_ID == a.ID
                                                select t.Test_Name).ToList()
                               };

            return View(appointments.ToList());
        }

        public ActionResult InventoryManagement()
        {
            return View();
        }
        ///////////////////////////////////////// Tests Documentation ///////////////////////////////////////////////
        public ActionResult TestsDocumentation()
        {
            var tests = db.Tests.ToList();
            return View(tests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTest(Test model) 
        {
            if (ModelState.IsValid)
            {
                db.Tests.Add(model);
                db.SaveChanges();

            }

            return RedirectToAction("TestsDocumentation");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTest(Test model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
               
            }
            return RedirectToAction("TestsDocumentation");
        }

        [HttpPost]
        public ActionResult DeleteTest(int Test_ID)
        {
            var test = db.Tests.Find(Test_ID);
            if (test != null)
            {
                db.Tests.Remove(test);
                db.SaveChanges();
            }
            return RedirectToAction("TestsDocumentation");
        }

        public ActionResult TestsDocumentationssssssssssss()
        {
            return View();
        }

        public ActionResult TestDocumentationADD()
        {
            return View();
        }



        //////////////////////////////////////////// Emloyee Page ///////////////////////////////////
      
        public ActionResult Employees()
        {
            var employees = db.Lab_Tech.ToList();
            return View(employees);
        }
        [HttpPost]
        public ActionResult CreateEmployee(Lab_Tech employee)
        {
            if (ModelState.IsValid) 
            {
                db.Lab_Tech.Add(employee);
                db.SaveChanges();

                // Redirect to the list of employees (or wherever you want)
                return RedirectToAction("Employees");
            }

            return View(employee);
        }

        [HttpPost]
        public ActionResult EditEmployee(Lab_Tech employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Employees");
            }
            return View(employee);
        }

        [HttpPost]
        public ActionResult DeleteEmployee(int Tech_ID)
        {
            var employee = db.Lab_Tech.Find(Tech_ID);
            if (employee != null)
            {
                db.Lab_Tech.Remove(employee);
                db.SaveChanges();
            }
            return RedirectToAction("Employees");
        }


        //////////////////////////////////////////////Profile Page/////////////////////////////////////////


        public ActionResult Profile(int id)
        {
            var employee = db.Lab_Tech.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(Lab_Tech updatedEmployee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(updatedEmployee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Profile", new { id = updatedEmployee.Tech_ID });
            }
            return View("EmployeeProfile", updatedEmployee);
        }


        ////////////////////////////////////////////////////  Packages   ////////////////////////////////////////////////

        public ActionResult Packages()
        {
            var packages = db.Packages.Include(p => p.Package_Tests.Select(pt => pt.Test)).ToList();
            ViewBag.TestsList= db.Tests.ToList();
            return View(packages);
        }

        // This action gets the selected tests for a specific package via Ajax
        public JsonResult GetPackageDetails(int id)
        {
            var package = db.Packages.Include(p => p.Package_Tests.Select(pt => pt.Test))
                                      .FirstOrDefault(p => p.Package_ID == id);
            var selectedTests = package.Package_Tests.Select(pt => new {
                pt.Test.Test_Name,
                pt.Test.Price,
                pt.Test.Test_ID
            }).ToList();

            return Json(new { selectedTests = selectedTests }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePackage(PackageAndTests model, HttpPostedFileBase Picture)
        {
            if (ModelState.IsValid)
            {
                // Check if a picture was uploaded
                if (Picture != null && Picture.ContentLength > 0)
                {
                    // Generate a unique filename and save the file
                    var fileName = Path.GetFileName(Picture.FileName);
                    var path = Path.Combine(Server.MapPath("~/Uploads/Packages"), fileName);

                    // Save the file to the server
                    Picture.SaveAs(path);

                    // Save the path to the database
                    model.Picture = fileName; // Assign the file path to the model's Picture property
                }
                var package = new Package
                {
                    Package_Name = model.Package_Name,
                    Description = model.Description,
                    Price = model.Price,
                    Picture = model.Picture,
                    Old_price = model.Old_price,
                };
                db.Packages.Add(package);
                db.SaveChanges();

                foreach(var test in model.SelectedTests)
                {
                    var t = new Package_Tests
                    {
                        Package_ID = package.Package_ID,
                        Test_ID = test.Test_ID,
                    };
                    db.Package_Tests.Add(t);
                }
                db.SaveChanges();

                // Save the package data to the database, including the picture path

                return RedirectToAction("Packages");
            }

            // If something went wrong, return the model to the view
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPackage(PackageAndTestsEDIT model, HttpPostedFileBase Picture)
        {
            if (ModelState.IsValid)
            {
                // Check if a picture was uploaded
                if (Picture != null && Picture.ContentLength > 0)
                {
                    // Generate a unique filename and save the file
                    var fileName = Path.GetFileName(Picture.FileName);
                    var path = Path.Combine(Server.MapPath("~/Uploads/Packages"), fileName);

                    // Save the file to the server
                    Picture.SaveAs(path);

                    // Save the path to the database
                    model.Picture = fileName; // Assign the file path to the model's Picture property
                }
                // Update the package details
                var package = db.Packages.Find(model.Package_ID);
                if (package == null)
                {
                    return HttpNotFound();
                }

                package.Package_Name = model.Package_Name;
                package.Description = model.Description;
                package.Price = model.Price;
                package.Picture = model.Picture;
                // Handle image upload if necessary

                // Remove old tests from the package
                db.Package_Tests.RemoveRange(package.Package_Tests);

                

                foreach (var test in model.SelectedTestsEDIT)
                {
                    var t = new Package_Tests
                    {
                        Package_ID = package.Package_ID,
                        Test_ID = test.Test_ID,
                    };
                    db.Package_Tests.Add(t);
                }
                db.SaveChanges();

                db.Entry(package).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Packages");
            }

            // If something goes wrong, return the view with the model to show errors
            return RedirectToAction("Packages");
        }

        [HttpPost]
        public ActionResult DeletePackage(int Package_ID)
        {
            var package = db.Packages.Find(Package_ID);

            if (package == null)
            {
                return HttpNotFound();
            }

            var packageTests = db.Package_Tests.Where(pt => pt.Package_ID == Package_ID).ToList();

            db.Package_Tests.RemoveRange(packageTests);

            db.Packages.Remove(package);

            db.SaveChanges();

            return RedirectToAction("Packages");
        }



        


        ///////////////////////////////////////////////////////          FeedBack          ///////////////////////////////////////////////////////////

        public ActionResult FeedBacks()
        {
            var feed = db.Feedbacks.ToList();
            return View(feed);
        }
        public ActionResult ApproveFeedback(int id)
        {
            var feedback = db.Feedbacks.Find(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }

            feedback.Status = "Approved";
            db.SaveChanges();

            return RedirectToAction("FeedBacks");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFeedback(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.Status = "Pending"; // Default status when feedback is created
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                return RedirectToAction("Profile", "User");
            }

            // If there's an issue with the model state, return the same view with validation errors
            return View(feedback);
        }

        [HttpPost]
        public ActionResult DeleteFeedback(int Feedback_ID)
        {
            // Find the feedback by ID
            var feedback = db.Feedbacks.Find(Feedback_ID);

            // Check if feedback exists
            if (feedback == null)
            {
                return HttpNotFound();
            }

            // Remove the feedback from the database
            db.Feedbacks.Remove(feedback);
            db.SaveChanges();

            // Redirect to the feedback list page
            return RedirectToAction("FeedBacks");
        }


        ////////////////////////////////////////////  Contact Us ////////////////////////////////////////////
        
        public ActionResult ContactAdmin()
        {
            var contact = db.Contacts.OrderByDescending(l => l.sent_date).ToList();

        return View(contact); 
        
        }

        public ActionResult contactDetails(int contactID)
        {
            var contact = db.Contacts.Find(contactID);
            return View(contact);
        }

        [HttpPost]
        public ActionResult contactDetails(Contact contact)
        {
            var contactPUT = db.Contacts.Find(contact.contact_id);
            contactPUT.admin_response = contact.admin_response;
            contactPUT.response_date = DateTime.Now;
            contactPUT.status = 1;

            db.Entry(contactPUT).State = EntityState.Modified;
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
                        <pre>{contactPUT.admin_response}</pre>
                        <p>If you have any more questions, feel free to reach out to us.</p>
                        <p>With best regards,<br>Admin</p>
                    </body>
                    </html>";
                string toEmail = contactPUT.email;
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
                return RedirectToAction("ContactAdmin");
            }
            catch
            {
                return RedirectToAction("ContactAdmin");
            }
        }

        //////////////////////////////////////////// doctor side ////////////////////////////////////////////


        public ActionResult DoctorDashboard()
        {
            return View();
        }
        public ActionResult DoctorReport()
        {
            var ordersWithDoctorReport = db.Test_Order
                   .Where(order => order.Test_Order_Tests.Any(t => t.Test.Test_Name == "Doctor Report"))
                   .ToList(); 
            
            return View(ordersWithDoctorReport);
        }

        public ActionResult DoctorReportAdd(int orderID)
        {
            var order = db.Test_Order.Find(orderID);
            return View(order);
        }

        [HttpPost]
        public ActionResult saveDoctorReport(int orderID, string DoctorReport)
        {
            var test = db.Test_Order_Tests.Where(t => t.Order_ID == orderID && t.Test.Test_Name == "Doctor Report").FirstOrDefault();
            test.Result = DoctorReport;
            test.Status = "Completed";
            db.Entry(test).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("DoctorReportAdd", new { orderID = orderID });

        }


        public ActionResult patientDoctor(int patientID)
        {
            var patient = new PatientAndTests
            {
                Patients = db.Patients.Find(patientID),
                TestOrders = db.Test_Order.Where(p => p.Patient_ID == patientID).ToList()
            };
            return View(patient);

        }



    }
}