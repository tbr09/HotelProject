using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Trip
    {
        public Attraction sourceAtt { get; set; }
        public Attraction destinationAtt { get; set; }
        public double distance { get; set; }

        public Trip(Attraction _att, Attraction _att2, double _dist)
        {
            sourceAtt = _att;
            destinationAtt = _att2;
            distance = _dist;
        }
    }
}