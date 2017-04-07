using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Hotel
    {
        public string id { get; set; }
        public geometry geometry { get; set; }
        public string name { get; set; }
        public decimal rating { get; set; }
        public List<String> types { get; set; } 
    }
}