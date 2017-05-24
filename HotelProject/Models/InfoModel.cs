using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class InfoModel
    {
        public int days { get; set; }
        public int distanceLimit { get; set; }
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