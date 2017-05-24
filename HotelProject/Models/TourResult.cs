using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class TourResult
    {
        public List<Travel> tour { get; set; }
        public double score { get; set; }

        public TourResult()
        {
            tour = new List<Travel>();
        }
    }
}