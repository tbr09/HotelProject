using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class MapViewModel
    {
        public List<Point> attractions { get; set; }
        public List<Point> hotels { get; set; }
        public InfoModel infoModel { get; set; }
        public TourResult tourResult { get; set; }

        public MapViewModel()
        {
            tourResult = new TourResult();
            infoModel = new InfoModel();
        }

        public MapViewModel(List<Point> _attractions, List<Point> _hotels)
        {
            tourResult = new TourResult();
            infoModel = new InfoModel();
            attractions = _attractions;
            hotels = _hotels;
        }
    }
}