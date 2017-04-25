using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class geometry
    {
        public location location { get; set; }

        public geometry()
        {
            location = new location();
        }
    }
}