using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.ViewModel
{
    public class PatientAndTests
    {
        public Patient Patients { get; set; }
        public IEnumerable<Test_Order> TestOrders { get; set; }
    }
}