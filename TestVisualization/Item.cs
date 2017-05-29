using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestVisualization
{
    public class Item
    {
        public double lat { get; set; }
        public double lng { get; set; }
        public double rating { get; set; }

        public Item() { }

        public Item(double _lat, double _lng, double _rating)
        {
            lat = _lat;
            lng = _lng;
            rating = _rating;
        }
    }
}
