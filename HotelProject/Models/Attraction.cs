using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Attraction
    {
        //public decimal lat { get; set; }
        //public decimal lng { get; set; }
        public string id { get; set; }
        public geometry geometry { get; set; }
        public string name { get; set; }
        public decimal rating { get; set; }
        public List<String> types { get; set; }
        public bool visited { get; set; }
        public Attraction()
        {
            visited = false;
        }
    }
}