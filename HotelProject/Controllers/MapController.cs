﻿using HotelProject.Models;
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
        public List<Point> twoOpt(int x, int y, List<Point> pointList)
        {
            int k = 0;
            List<Point> reversedList = new List<Point>();
            List<Point> newList = new List<Point>();
            for (int i = 0; i < y; i++)
            {
                if (i > x - 1 && i < y - 1)
                {
                    reversedList.Add(pointList[i]);
                }
            }
            reversedList.Reverse();
            for (int i = 0; i < pointList.Count; i++)
            {
                if (i > x - 1 && i < y - 1)
                {
                    newList.Add(reversedList[k]); k++;
                }
                else newList.Add(pointList[i]);
            }
            return newList;
        }


        //test data
        public Travel Alg1()
        {
            StreamReader sr = new StreamReader(Server.MapPath("~/F.txt"));

            string s;
            string[] separators = { " ", "," };
            List<Point> attList = new List<Point>();
            List<Point> hotelList = new List<Point>();
            s = sr.ReadLine();
            string[] words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            int N = int.Parse(words[0]);
            int days = int.Parse(words[1]);
            double distanceLimit = int.Parse(words[2]);

            #region 
            do
            {
                s = sr.ReadLine();
                if (s != null)
                {
                    words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    Point newPoint = new Point();
                    newPoint.geometry.location.lat = Decimal.Parse(words[0], System.Globalization.NumberStyles.Float);
                    newPoint.geometry.location.lng = Decimal.Parse(words[1], System.Globalization.NumberStyles.Float);
                    if (words.Length == 3)
                    {
                        newPoint.rating = decimal.Parse(words[2]);
                        attList.Add(newPoint);
                    }
                    else hotelList.Add(newPoint);
                }
            }
            while (s != null);
            #endregion

            DateTime start;
            DateTime stop;
            Random rand = new Random();

            start = DateTime.Now;
            List<Item>[] LI = new List<Item>[400];
            List<Item>[] hotelsLI = new List<Item>[20];
            GenerateLI(LI, attList);
            GenerateHotLI(hotelsLI, attList, hotelList);
            stop = DateTime.Now;
            Debug.WriteLine("Generating LI Time->" + (stop - start).TotalMilliseconds + "ms");

            start = DateTime.Now;
            List<Trip> trip = new List<Trip>();
            List<Point> visited = new List<Point>();

            Travel travel = new Travel(hotelList[0], hotelList[0]);

            int i = 0;
            int ind = hotelList.IndexOf(travel.sourceHotel);
            Point current = hotelsLI[ind].First().direction;
            while (i < 400 && travel.totalDistance < distanceLimit)
            {
                while (visited.Contains(current))
                {
                    LI[ind].RemoveAt(0);
                    if (LI[ind].Count == 0) break;
                    current = LI[ind].First().direction;
                }
                ind = attList.IndexOf(current);
                if (!visited.Contains(current))
                {
                    if (travel.totalDistance + Math.Floor(DistanceBetweenPlaces(current, LI[ind].First().direction)) < distanceLimit)
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
            Debug.WriteLine("Rating ->" + travel.totalRating);

            return travel;
        }



        //real coords
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
            Random rand = new Random();

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
            double distanceLimit = 400;

            Point sourceHot = hot.Where(k => k.name == "Hotel Leśna").Single();
            Point destinationHot = hot.Where(k => k.name == "Hotel Ostrów Mazowiecka").Single();
            Travel travel = new Travel(sourceHot, destinationHot);

            //greedy 
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


            foreach (var x in travel.attractionList)
            {
                Debug.Write(x.ToString() + " -> ");
            }

            //two-opt
            start = DateTime.Now;
            Travel tempTravel = new Travel(travel.sourceHotel, travel.destinationHotel);
            List<Point> tempList = travel.attractionList;
            i = 0;
            int k1 = 0, k2 = 0;
            int iterations = 6000;
            int checkingNumber;
            double oldDistance = travel.totalDistance;
            while (i < iterations)
            {
                //random checkingNumber (optional)
                checkingNumber = rand.Next(1, 8);
                k1 = rand.Next(0, travel.attractionList.Count - checkingNumber);
                k2 = k1 + rand.Next(0, checkingNumber);
                tempTravel.attractionList = twoOpt(k1, k2, travel.attractionList);
                tempTravel.totalDistance = tempTravel.CalculateDistance();
                if (tempTravel.totalDistance < travel.totalDistance)
                {
                    travel.attractionList = tempTravel.attractionList;
                    travel.totalDistance = tempTravel.totalDistance;
                }
                i++;
            }
            stop = DateTime.Now;
            Debug.WriteLine("\nTwoOpt Time(" + iterations + "-iterations)->" + (stop - start).TotalMilliseconds + "ms");
            Debug.WriteLine("Old Distance->" + oldDistance + "  New Distance->" + travel.totalDistance);

            
            return travel;
        }
    }
}