using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelProject.Models
{
    public class Travel
    {
        public Point sourceHotel { get; set; }
        public Point destinationHotel { get; set; }
        public List<Point> attractionList { get; set; }
        public double totalDistance { get; set; }
        public decimal totalRating { get; set; }



        const double RADIUS = 6378.16;

        public static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        public static double DistanceBetweenPlaces(Point a1, Point a2)
        {
            double lat1 = (double)a1.geometry.location.lat;
            double lat2 = (double)a2.geometry.location.lat;
            double lon1 = (double)a1.geometry.location.lng;
            double lon2 = (double)a2.geometry.location.lng;

            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return angle * RADIUS;
        }


        public Travel(Point _srchotel, Point _dsthotel)
        {
            attractionList = new List<Point>();
            sourceHotel = _srchotel;
            destinationHotel = _dsthotel;
        }
        public Travel() { }

        public double CalculateDistance()
        {
            totalDistance = 0;
            totalDistance += DistanceBetweenPlaces(sourceHotel, attractionList[0]);

            for (int i = 1; i < attractionList.Count - 2; i++)
            {
                totalDistance += DistanceBetweenPlaces(attractionList[i], attractionList[i + 1]);
            }

            totalDistance += DistanceBetweenPlaces(attractionList[attractionList.Count - 1], destinationHotel);

            return totalDistance;
        }


        public double Euclidean(Point a1, Point a2)
        {
            //for testing data (Euclidean)
            //double lat1 = (double)a1.geometry.location.lat;
            //double lat2 = (double)a2.geometry.location.lat;
            //double lon1 = (double)a1.geometry.location.lng;
            //double lon2 = (double)a2.geometry.location.lng;
            //return Math.Sqrt(Math.Pow(lon1 - lon2, 2) + Math.Pow(lat1 - lat2, 2));


            //for real coords
            double lat1 = (double)a1.geometry.location.lat;
            double lat2 = (double)a2.geometry.location.lat;
            double lon1 = (double)a1.geometry.location.lng;
            double lon2 = (double)a2.geometry.location.lng;
            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);
            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return angle * RADIUS;
        }

        public void addAtt(Point _att)
        {
            attractionList.Add(_att);
            totalRating += _att.rating;
            totalDistance = CalculateDistance();
        }


        public double DistanceWithAtt(Point _att, int x)
        {
            Point tempDelete = attractionList[x];
            attractionList.RemoveAt(x);
            attractionList.Insert(x, _att);
            double dist = CalculateDistance();
            attractionList.RemoveAt(x);
            attractionList.Insert(x, tempDelete);
            totalDistance = CalculateDistance();
            return dist;

        }
        public decimal RatingWithAtt(Point _att)
        {
            return totalRating + _att.rating;
        }
    }
}