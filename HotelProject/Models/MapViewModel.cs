using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class MapViewModel
    {
        public List<Travel> tour { get; set; }
        public List<Point> attractions { get; set; }
        public List<Point> hotels { get; set; }
    }
}