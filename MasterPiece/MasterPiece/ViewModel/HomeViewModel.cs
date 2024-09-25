using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.ViewModel
{
    public class HomeViewModel
    {
        public IEnumerable<Package> Package { get; set; }
        public IEnumerable<Feedback> Feedback { get; set; }
    }
}