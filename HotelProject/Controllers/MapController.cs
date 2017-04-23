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

        public static double DistanceBetweenPlaces(Attraction a1, Attraction a2)
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

        public List<Trip> Alg()
        {
            List<Attraction> att = new List<Attraction>();
            using (StreamReader r = new StreamReader(Server.MapPath("~/data.json")))
            {
                string json = r.ReadToEnd();
                att = JsonConvert.DeserializeObject<List<Attraction>>(json);
            }
            List<Hotel> hot = new List<Hotel>();
            using (StreamReader r = new StreamReader(Server.MapPath("~/hotels.json")))
            {
                string json = r.ReadToEnd();
                hot = JsonConvert.DeserializeObject<List<Hotel>>(json);
            }
            //att.Where(k=>k.name = "")
            List<Item>[] LI = new List<Item>[400];
            int i = 0;
            DateTime start;
            DateTime stop;

            start = DateTime.Now;
            foreach (Attraction item in att)
            {
                LI[i] = new List<Item>();
                foreach (Attraction item1 in att)
                {
                    if (item1 != item) LI[i].Add(new Item(item1, DistanceBetweenPlaces(item, item1)));
                }
                LI[i] = LI[i].OrderBy(k => k.distance).ToList();
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("Time->" + (stop - start).TotalMilliseconds+"ms");
            start = DateTime.Now;
            List<Trip> trip = new List<Trip>();
            List<Attraction> visited = new List<Attraction>();
            int ind;
            int s = 13;
            i = 0;
            Attraction current = att[s];
            double totalDistance = 0;
            decimal totalScore = 0;
            double distanceLimit = 600;
            ind = att.IndexOf(current);
            while (i < 400 && totalDistance < distanceLimit)
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
                    if (totalDistance + DistanceBetweenPlaces(current, LI[ind].First().direction) < distanceLimit)
                    {
                        trip.Add(new Trip(current, LI[ind].First().direction, DistanceBetweenPlaces(current, LI[ind].First().direction)));
                        totalDistance += DistanceBetweenPlaces(current, LI[ind].First().direction);
                        totalScore += current.rating;
                        visited.Add(current);
                        LI[ind].RemoveAt(0);
                    }
                }
                current = LI[ind].First().direction;

                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("Time->"+(stop - start).TotalMilliseconds+"ms");
            Debug.WriteLine("Total distance->" + totalDistance);
            Debug.WriteLine("Total score->" + totalScore);
            return trip;
            //Debug.WriteLine(DistanceBetweenPlaces(att.Where(k => k.name == "Aeroklub Białystok Krywlany").Single(), att.Where(k => k.name == "Aeroklub Elbląg").Single()));
        }
    }
}