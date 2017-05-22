using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Point
    {
        public string id { get; set; }
        public geometry geometry { get; set; }

        [Range(1,5)]
        public decimal rating { get; set; }
        public string name { get; set; }

        public Point()
        {
            geometry = new geometry();
        }
        public Point(geometry geometry, decimal rating, string name)
        {
            this.name = name;
            this.geometry = geometry;
            this.rating = rating;
        }

        public override string ToString()
        {
            return name + "(" + rating + ")";
        }
    }
}