using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterPiece.Controllers
{
    public class NotificationsController : Controller
    {
        private MasterPieceEntities db = new MasterPieceEntities();
        // GET: Notifications
        [HttpGet]
        public JsonResult GetLatest()
        {
            // Step 1: Fetch the data from the database without formatting the date in the LINQ query
            var notifications = db.Notifications
                .Where(n => n.IsRead == false)
                .OrderByDescending(n => n.Notification_Date)
                .Take(10)
                .ToList(); // Materialize the data into memory

            // Step 2: Format the date after the data has been fetched
            var formattedNotifications = notifications.Select(n => new {
                n.Order_ID,
                Notification_Date = n.Notification_Date.HasValue ? n.Notification_Date.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null
            }).ToList();

            // Step 3: Return the formatted notifications
            return Json(formattedNotifications, JsonRequestBehavior.AllowGet);

        }
    }
}