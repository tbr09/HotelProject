using HotelProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelProject.Controllers
{
    public class MapController : Controller
    {
        // GET: Map
        public ActionResult Index()
        {
            StreamReader r = new StreamReader(Server.MapPath("~/data.json"));
            StreamReader x = new StreamReader(Server.MapPath("~/hotels.json"));
            string jsonAttractions = r.ReadToEnd();
            string jsonHotels = x.ReadToEnd();
            //var vm = new MapViewModel
            //{
            //    atractions = JsonConvert.DeserializeObject<List<Attraction>>(jsonAttractions),
            //    hotels = JsonConvert.DeserializeObject<List<Hotel>>(jsonHotels)
            //};
            return View(Alg());

        }

        public double Distance(Attraction a1, Attraction a2)
        {
            double x1 = (double)a1.geometry.location.lat;
            double x2 = (double)a2.geometry.location.lat;
            double y1 = (double)a1.geometry.location.lng;
            double y2 = (double)a2.geometry.location.lng;

            double x = Math.Sqrt(Math.Pow(Math.Pow((x2 - x1), 2) + (Math.Cos((x1 * Math.PI) / 180) * (y2 - y1)), 2)) * (40075.704 / 360.0);

            return x;
        }



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

        public void GenerateHotLI(List<Item>[] LI, List<Point> att, List<Point> hot)
        {
            int i = 0;
            foreach (Point item in hot)
            {
                LI[i] = new List<Item>();

                foreach (Point item1 in att)
                {
                    LI[i].Add(new Item(item1, DistanceBetweenPlaces(item, item1)));
                }
                foreach (Point item1 in hot)
                {
                    if (item1 != item) LI[i].Add(new Item(item1, DistanceBetweenPlaces(item, item1)));
                }
                LI[i] = LI[i].OrderBy(k => k.distance).ToList();

                i++;
            }
        }
        public void GenerateLI(List<Item>[] LI, List<Point> att)
        {
            int i = 0;
            foreach (Point item in att)
            {
                LI[i] = new List<Item>();
                foreach (Point item1 in att)
                {
                    if (item1 != item) LI[i].Add(new Item(item1, DistanceBetweenPlaces(item, item1)));
                }
                LI[i] = LI[i].OrderBy(k => k.distance).ToList();
                i++;
            }
        }
        public Travel Alg()
        {
            List<Point> att = new List<Point>();
            using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
            {
                string json = r.ReadToEnd();
                att = JsonConvert.DeserializeObject<List<Point>>(json);
            }

            List<Point> hot = new List<Point>();
            using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
            {
                string json = r.ReadToEnd();
                hot = JsonConvert.DeserializeObject<List<Point>>(json);
            }
            
            DateTime start;
            DateTime stop;

            start = DateTime.Now;
            List<Item>[] LI = new List<Item>[400];
            List<Item>[] hotelsLI = new List<Item>[20];
            GenerateLI(LI, att);
            GenerateHotLI(hotelsLI, att, hot);
            stop = DateTime.Now;
            Debug.WriteLine("Generating LI Time->" + (stop - start).TotalMilliseconds + "ms");

            start = DateTime.Now;
            List<Trip> trip = new List<Trip>();
            List<Point> visited = new List<Point>();
            double distanceLimit = 200;

            Point sourceHot = hot.Where(k => k.name == "Hotel Branicki ****").Single();
            Point destinationHot = hot.Where(k => k.name == "Hotel Ostrów Mazowiecka").Single();
            Travel travel = new Travel(sourceHot, destinationHot);

            int i = 0;
            int ind = hot.IndexOf(sourceHot);
            Point current = hotelsLI[ind].First().direction;
            while (i < 400 && travel.totalDistance < distanceLimit)
            {
                while (visited.Contains(current))
                {
                    LI[ind].RemoveAt(0);
                    if (LI[ind].Count == 0) break;
                    current = LI[ind].First().direction;
                }
                ind = att.IndexOf(current);
                if (!visited.Contains(current))
                {
                    if (travel.totalDistance + DistanceBetweenPlaces(current, LI[ind].First().direction) < distanceLimit)
                    {
                        travel.addAtt(current);
                        visited.Add(current);
                        LI[ind].RemoveAt(0);
                    }
                }
                current = LI[ind].First().direction;
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("Greedy Time->" + (stop - start).TotalMilliseconds + "ms");
            return travel;

        }
    }
}