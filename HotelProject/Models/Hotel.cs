using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Hotel : Point
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<String> types { get; set; }
        
        public Hotel() {}
        public Hotel(Attraction _att) : base(_att.geometry, _att.rating, _att.name)
        {
            id = _att.id;
            name = _att.name;
            types = _att.types;
        }
    }
}