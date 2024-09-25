using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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