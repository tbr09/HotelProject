using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class MapViewModel
    {
        public IEnumerable<Attraction> atractions { get; set; }
        public IEnumerable<Hotel> hotels { get; set; }
    }
}