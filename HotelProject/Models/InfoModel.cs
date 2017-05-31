using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class InfoModel
    {
        [Display(Name = "Days")]
        public int days { get; set; }
        [Display(Name = "Distance limit")]
        public int distanceLimit { get; set; }
        [Display(Name = "Algorithm time")]
        public int seconds { get; set; }

        public InfoModel() { }

        public InfoModel(int _days, int _distLimit, int _secs)
        {
            seconds = _secs;
            days = _days;
            distanceLimit = _distLimit;
        }
    }
}