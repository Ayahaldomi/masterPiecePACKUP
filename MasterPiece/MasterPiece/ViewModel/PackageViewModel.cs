using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.ViewModel
{
    public class PackageViewModel
    {
        public int Package_ID { get; set; }
        public string Package_Name { get; set; }
        public decimal? Price { get; set; }
        public string Tests { get; set; }
    }
}