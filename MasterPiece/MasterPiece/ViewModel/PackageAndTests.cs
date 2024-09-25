using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.ViewModel
{
    public class PackageAndTests
    {
        public string Package_Name { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Picture { get; set; }
        public Nullable<decimal> Old_price { get; set; }
        public List<Package_Tests> SelectedTests { get; set; }

    }
}