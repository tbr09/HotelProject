using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class location
    {
        [Range(-80,80)]
        public decimal lat { get; set; }
        [Range(-180,180)]
        public decimal lng { get; set; }
    }
}