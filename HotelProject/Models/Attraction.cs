using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Attraction : Point
    {
        //public decimal lat { get; set; }
        //public decimal lng { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public List<String> types { get; set; }
        public bool visited { get; set; }
       
        public Attraction() { }
        public Attraction(Attraction _att) : base(_att.geometry, _att.rating, _att.name)
        {
            id = _att.id;
            name = _att.name;
            types = _att.types;
        }
    }
}