using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.ViewModel
{
    public class AppointmentPOST
    {
        public string Full_Name { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> Date_Of_Birth { get; set; }
        public string Email_Address { get; set; }
        public string Phone_Number { get; set; }
        public string Home_Address { get; set; }
        public Nullable<System.DateTime> Date_Of_Appo { get; set; }
        public Nullable<decimal> Total_price { get; set; }
        public Nullable<decimal> Amount_paid { get; set; }
        public Nullable<int> Billing_ID { get; set; }
        public string Status { get; set; }
        public List<Appointments_Tests> SelectedTests { get; set; }

    }
}