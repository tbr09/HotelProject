using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class MainViewModel
    {
        public IEnumerable<Point> attractions { get; set; }
        public IEnumerable<Point> hotels { get; set; }
    }
}