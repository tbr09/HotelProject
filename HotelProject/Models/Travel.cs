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
        public double score
        {
            get
            {
                return (double)totalRating / totalDistance;
            }
        }

        const double RADIUS = 6378.16;

        public static double Radians(double x)
        {
            return x * Math.PI / 180;
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
            double totalDistance1 = 0;
            totalDistance1 += DistanceBetweenPlaces(sourceHotel, attractionList[0]);

            for (int i = 0; i < attractionList.Count - 1; i++)
            {
                totalDistance1 += DistanceBetweenPlaces(attractionList[i], attractionList[i + 1]);
            }

            totalDistance1 += DistanceBetweenPlaces(attractionList[attractionList.Count - 1], destinationHotel);

            return totalDistance1;
        }


        public double DistanceBetweenPlaces(Point a1, Point a2)
        {
            //for testing data (Euclidean)

            double lat1 = (double)a1.geometry.location.lat;
            double lat2 = (double)a2.geometry.location.lat;
            double lon1 = (double)a1.geometry.location.lng;
            double lon2 = (double)a2.geometry.location.lng;
            return Math.Sqrt(Math.Pow(lon1 - lon2, 2) + Math.Pow(lat1 - lat2, 2));


            //for real coords

            //double lat1 = (double)a1.geometry.location.lat;
            //double lat2 = (double)a2.geometry.location.lat;
            //double lon1 = (double)a1.geometry.location.lng;
            //double lon2 = (double)a2.geometry.location.lng;
            //double dlon = Radians(lon2 - lon1);
            //double dlat = Radians(lat2 - lat1);
            //double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            //double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //return angle * RADIUS;
        }

        public void addAtt(Point _att, List<Item>[] distanceLI)
        {
            attractionList.Add(_att);
            totalRating += _att.rating;
            //totalDistance += distanceLI[ind].Last().distance;
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