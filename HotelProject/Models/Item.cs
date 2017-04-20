using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Item
    {
        public Attraction direction { get; set; }
        public double distance { get; set; }


        public Item(Attraction _att, double _distance)
        {
            direction = _att;
            distance = _distance;
        }
    }
}